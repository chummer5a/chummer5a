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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectLifeModule : Form
    {
        public bool AddAgain { get; private set; }
        private readonly Character _objCharacter;
        private readonly int _intStage;
        //private XmlDocument _xmlDocument;
        private string _selectedId;
        private Regex _searchRegex;

        private readonly XPathNavigator _xmlBaseQualityDataNode;
        private readonly List<ListItem> _lstStage = new List<ListItem>();

        //private string _strWorkStage;
        
        public frmSelectLifeModule(Character objCharacter, int stage = 0)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _intStage = stage;
            MoveControls();
            // Load the Quality information.
            _xmlBaseQualityDataNode = XmlManager.Load("lifemodules.xml").GetFastNavigator().SelectSingleNode("/chummer");
        }

        private void frmSelectLifeModule_Load(object sender, EventArgs e)
        {
            // Populate the Quality Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseQualityDataNode.Select("stages/stage"))
            {
                string strInnerText = objXmlCategory.Value;
                _lstStage.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }

            if (_lstStage.Count > 0)
            {
                _lstStage.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            cboStage.BeginUpdate();
            cboStage.ValueMember = "Value";
            cboStage.DisplayMember = "Name";
            cboStage.DataSource = _lstStage;
            cboStage.SelectedIndex = _intStage;
            cboStage.EndUpdate();
            BuildTree();
        }

        private void BuildTree()
        {
            string strCategory = cboStage.SelectedValue?.ToString() ?? string.Empty;
            StringBuilder strFilter = new StringBuilder("(");
            strFilter.Append(_objCharacter.Options.BookXPath());
            strFilter.Append(')');
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
            {
                strFilter.Append(" and stage = \"");
                strFilter.Append(strCategory);
                strFilter.Append('\"');
            }

            XPathNodeIterator matches = _xmlBaseQualityDataNode.Select("modules/module[" + strFilter + ']');
            treModules.Nodes.Clear();
            treModules.Nodes.AddRange(
                BuildList(matches));
        }

        private TreeNode[] BuildList(XPathNodeIterator xmlNodes, bool ignoreSearchRegex = false)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (XPathNavigator xmlNode in xmlNodes)
            {
                if (!chkLimitList.Checked || xmlNode.RequirementsMet(_objCharacter))
                {

                    TreeNode treNode = new TreeNode
                    {
                        Text = xmlNode.SelectSingleNode("name")?.Value ?? string.Empty,
                        Tag = xmlNode.SelectSingleNode("id")?.Value
                    };
                    if (!ignoreSearchRegex && _searchRegex != null && !_searchRegex.IsMatch(treNode.Text))
                    {
                        if (xmlNode.SelectSingleNode("versions") != null)
                        {
                            treNode.Nodes.AddRange(BuildList(xmlNode.Select("versions/version[" + _objCharacter.Options.BookXPath() + "or not(source)]")));
                        }
                        if (treNode.Nodes.Count == 0)
                        {
                            continue;
                        }

                    }
                    else if (xmlNode.SelectSingleNode("versions") != null)
                    {
                        treNode.Nodes.AddRange(BuildList(xmlNode.Select("versions/version[" + _objCharacter.Options.BookXPath() + "or not(source)]"), true));
                    }
                    nodes.Add(treNode);
                }
            }
            return nodes.ToArray();
        }
        
        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblBPLabel.Width, lblSourceLabel.Width);
            lblBP.Left = lblBPLabel.Left + intWidth + 6;
            lblSource.Left = lblSourceLabel.Left + intWidth + 6;

            lblSearch.Left = txtSearch.Left - 6 - lblSearch.Width;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(_selectedId))
                return;

            XmlNode objNode = SelectedNode;

            if (objNode == null || !objNode.RequirementsMet(_objCharacter, LanguageManager.GetString("String_LifeModules", GlobalOptions.Language)))
                return;

            DialogResult = DialogResult.OK;
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
                string selectString = "//*[id = \"" + e.Node.Tag + "\"]/selectable";
                XPathNavigator node = _xmlBaseQualityDataNode.SelectSingleNode(selectString);
                //if it contains >selectable>True</selectable>, yes or </selectable>
                //set button to selectable, otherwise not
                blnSelectAble = (node != null && (node.Value == bool.TrueString || node.OuterXml.EndsWith("/>")));
            }

            _selectedId = (string)e.Node.Tag;
            XmlNode selectedNodeInfo = Quality.GetNodeOverrideable(_selectedId, XmlManager.Load("lifemodules.xml", GlobalOptions.Language));

            if (selectedNodeInfo != null)
            {
                cmdOK.Enabled = blnSelectAble;
                cmdOKAdd.Enabled = blnSelectAble;

                lblBP.Text = selectedNodeInfo["karma"]?.InnerText ?? string.Empty;
                lblSource.Text = (selectedNodeInfo["source"]?.InnerText ?? string.Empty) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + selectedNodeInfo["page"]?.InnerText;
            }
            else
            {
                lblBP.Text = LanguageManager.GetString("String_Error", GlobalOptions.Language);
                lblSource.Text = LanguageManager.GetString("String_Error", GlobalOptions.Language);

                cmdOK.Enabled = false;
                cmdOKAdd.Enabled = false;
            }
        }

        public XmlNode SelectedNode => Quality.GetNodeOverrideable(_selectedId, XmlManager.Load("lifemodules.xml", GlobalOptions.Language));

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
            BuildTree();
        }

        private void cboStage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            BuildTree();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                _searchRegex = null;
            }
            else
            {
                try
                {
                    _searchRegex = new Regex(txtSearch.Text, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException)
                {
                    //No other way to check for a valid regex that i know of
                }
            }
            BuildTree();
        }
    }
}
