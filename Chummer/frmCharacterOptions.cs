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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer
{
    public partial class frmCharacterOptions : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly CharacterOptions _objCharacterOptions;
        private CharacterOptions _objReferenceCharacterOptions;
        // List of custom data directories on the character, in load order. If the character has a directory name for which we have no info, Item1 will be null
        private readonly List<Tuple<object, bool>> _lstCharacterCustomDataDirectoryInfos = new List<Tuple<object, bool>>();
        private bool _blnLoading = true;
        private bool _blnSkipLimbCountUpdate;
        private bool _blnDirty;
        private bool _blnSourcebookToggle = true;
        private readonly HashSet<string> _setPermanentSourcebooks = new HashSet<string>();

        #region Form Events
        public frmCharacterOptions(CharacterOptions objExistingOptions = null)
        {
            InitializeComponent();
            this.TranslateWinForm();
            _objReferenceCharacterOptions = objExistingOptions ?? OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultGameplayOption];
            _objCharacterOptions = new CharacterOptions(_objReferenceCharacterOptions);
        }


        private void frmCharacterOptions_Load(object sender, EventArgs e)
        {
            SetToolTips();
            PopulateSettingsList();

            List<ListItem> lstBuildMethods = new List<ListItem>(4)
            {
                new ListItem(CharacterBuildMethod.Priority, LanguageManager.GetString("String_Priority")),
                new ListItem(CharacterBuildMethod.SumtoTen, LanguageManager.GetString("String_SumtoTen")),
                new ListItem(CharacterBuildMethod.Karma, LanguageManager.GetString("String_Karma"))
            };
            if (GlobalOptions.LifeModuleEnabled)
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.LifeModule, LanguageManager.GetString("String_LifeModule")));

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.ValueMember = nameof(ListItem.Value);
            cboBuildMethod.DisplayMember = nameof(ListItem.Name);
            cboBuildMethod.DataSource = lstBuildMethods;
            cboBuildMethod.EndUpdate();

            SetupDataBindings();
            PopulateOptions();

            foreach (KeyValuePair<string, Tuple<int, bool>> kvpCustomDataDirectory in _objCharacterOptions.CustomDataDirectoryNames.OrderBy(x => x.Value.Item1))
            {
                CustomDataDirectoryInfo objLoopInfo = GlobalOptions.CustomDataDirectoryInfos.FirstOrDefault(x => x.Name == kvpCustomDataDirectory.Key);
                if (objLoopInfo != null)
                {
                    _lstCharacterCustomDataDirectoryInfos.Add(
                        new Tuple<object, bool>(
                            objLoopInfo,
                            kvpCustomDataDirectory.Value.Item2));
                }
                else
                {
                    _lstCharacterCustomDataDirectoryInfos.Add(
                        new Tuple<object, bool>(
                            kvpCustomDataDirectory.Key,
                            kvpCustomDataDirectory.Value.Item2));
                }
            }
            PopulateCustomDataDirectoryTreeView();

            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // TODO: Prompt to confirm delete

            try
            {
                File.Delete(_objReferenceCharacterOptions.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                return;
            }

            OptionsManager.LoadedCharacterOptions.Remove(_objReferenceCharacterOptions.FileName);
            KeyValuePair<string, CharacterOptions> kvpReplacementOption = OptionsManager.LoadedCharacterOptions.First(x => x.Value.BuiltInOption
                                                                                                                           && x.Value.BuildMethod == _objReferenceCharacterOptions.BuildMethod);
            foreach (Character objCharacter in Program.MainForm.OpenCharacters.Where(x => x.CharacterOptionsKey == _objReferenceCharacterOptions.FileName))
                objCharacter.CharacterOptionsKey = kvpReplacementOption.Key;
            _blnDirty = false;
            PopulateSettingsList();
        }

        private void cmdSaveAs_Click(object sender, EventArgs e)
        {
            // TODO: Prompt for name and file name

            if (_objCharacterOptions.Save())
            {
                CharacterOptions objNewCharacterOptions = new CharacterOptions();
                objNewCharacterOptions.CopyValues(_objCharacterOptions);
                OptionsManager.LoadedCharacterOptions.Add(Path.Combine(Utils.GetStartupPath, "settings", objNewCharacterOptions.FileName), objNewCharacterOptions);
                _objReferenceCharacterOptions = objNewCharacterOptions;
                cmdSaveAs.Enabled = false;
                cmdSave.Enabled = false;
                _blnDirty = false;
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            if (_objCharacterOptions.Save())
            {
                _objReferenceCharacterOptions.CopyValues(_objCharacterOptions);
                cmdSaveAs.Enabled = false;
                cmdSave.Enabled = false;
                _blnDirty = false;
            }
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedFile = cboSetting.SelectedValue?.ToString();
            if(string.IsNullOrEmpty(strSelectedFile) || !OptionsManager.LoadedCharacterOptions.TryGetValue(strSelectedFile, out CharacterOptions objNewOption))
                return;

            if (_blnDirty)
            {
                // TODO: Prompt to cancel if dirty
            }

            _blnLoading = true;
            _objReferenceCharacterOptions = objNewOption;
            _objCharacterOptions.CopyValues(objNewOption);
            PopulateOptions();
            _blnLoading = false;
            _blnDirty = false;
        }

        private void cmdRestoreDefaults_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if(MessageBox.Show(
                LanguageManager.GetString("Message_Options_RestoreDefaults"),
                LanguageManager.GetString("MessageTitle_Options_RestoreDefaults"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            _blnLoading = true;
            _objCharacterOptions.CopyValues(_objReferenceCharacterOptions);
            PopulateOptions();
            _blnDirty = false;
            _blnLoading = false;
        }

        private void cboLimbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || _blnSkipLimbCountUpdate)
                return;

            string strLimbCount = cboLimbCount.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strLimbCount))
            {
                _objCharacterOptions.LimbCount = 6;
                _objCharacterOptions.ExcludeLimbSlot = string.Empty;
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if (intSeparatorIndex == -1)
                {
                    _objCharacterOptions.LimbCount = Convert.ToInt32(strLimbCount, GlobalOptions.InvariantCultureInfo);
                    _objCharacterOptions.ExcludeLimbSlot = string.Empty;
                }
                else
                {
                    _objCharacterOptions.LimbCount = Convert.ToInt32(strLimbCount.Substring(0, intSeparatorIndex), GlobalOptions.InvariantCultureInfo);
                    _objCharacterOptions.ExcludeLimbSlot = intSeparatorIndex + 1 < strLimbCount.Length ? strLimbCount.Substring(intSeparatorIndex + 1) : string.Empty;
                }
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            if (_blnDirty)
            {
                // TODO: Prompt to save if dirty
                string text = LanguageManager.GetString("Message_Options_SaveForms");
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms");

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            DialogResult = DialogResult.Cancel;
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            _blnLoading = true;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (!_setPermanentSourcebooks.Contains(objNode.Tag.ToString()))
                {
                    objNode.Checked = _blnSourcebookToggle;
                }
            }
            _blnLoading = false;
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.Books));
            _blnSourcebookToggle = !_blnSourcebookToggle;
        }

        private void treSourcebook_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_blnLoading)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            string strBookCode = objNode.Tag.ToString();
            if (string.IsNullOrEmpty(strBookCode) || (_setPermanentSourcebooks.Contains(strBookCode) && !objNode.Checked))
            {
                _blnLoading = true;
                objNode.Checked = !objNode.Checked;
                _blnLoading = false;
                return;
            }
            if (objNode.Checked)
                _objCharacterOptions.Books.Add(strBookCode);
            else
                _objCharacterOptions.Books.Remove(strBookCode);
            _objCharacterOptions.RecalculateBookXPath();
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.Books));
        }

        private void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            _lstCharacterCustomDataDirectoryInfos.Reverse(intIndex - 1, 2);
            string strCustomDataDirectoryToUp = _objCharacterOptions.CustomDataDirectoryNames.FirstOrDefault(x => x.Value.Item1 == intIndex).Key ?? string.Empty;
            string strCustomDataDirectoryToDown = _objCharacterOptions.CustomDataDirectoryNames.FirstOrDefault(x => x.Value.Item1 == intIndex - 1).Key ?? string.Empty;
            if (!string.IsNullOrEmpty(strCustomDataDirectoryToUp) && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(strCustomDataDirectoryToUp, out var objCustomDataDirectoryToUp))
                _objCharacterOptions.CustomDataDirectoryNames[strCustomDataDirectoryToUp] = new Tuple<int, bool>(objCustomDataDirectoryToUp.Item1 - 1, objCustomDataDirectoryToUp.Item2);
            if (!string.IsNullOrEmpty(strCustomDataDirectoryToDown) && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(strCustomDataDirectoryToDown, out var objCustomDataDirectoryToDown))
                _objCharacterOptions.CustomDataDirectoryNames[strCustomDataDirectoryToDown] = new Tuple<int, bool>(objCustomDataDirectoryToDown.Item1 + 1, objCustomDataDirectoryToDown.Item2);
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryNames));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _lstCharacterCustomDataDirectoryInfos.Count)
                return;
            _lstCharacterCustomDataDirectoryInfos.Reverse(intIndex, 2);
            string strCustomDataDirectoryToUp = _objCharacterOptions.CustomDataDirectoryNames.FirstOrDefault(x => x.Value.Item1 == intIndex + 1).Key ?? string.Empty;
            string strCustomDataDirectoryToDown = _objCharacterOptions.CustomDataDirectoryNames.FirstOrDefault(x => x.Value.Item1 == intIndex).Key ?? string.Empty;
            if (!string.IsNullOrEmpty(strCustomDataDirectoryToUp) && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(strCustomDataDirectoryToUp, out var objCustomDataDirectoryToUp))
                _objCharacterOptions.CustomDataDirectoryNames[strCustomDataDirectoryToUp] = new Tuple<int, bool>(objCustomDataDirectoryToUp.Item1 - 1, objCustomDataDirectoryToUp.Item2);
            if (!string.IsNullOrEmpty(strCustomDataDirectoryToDown) && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(strCustomDataDirectoryToDown, out var objCustomDataDirectoryToDown))
                _objCharacterOptions.CustomDataDirectoryNames[strCustomDataDirectoryToDown] = new Tuple<int, bool>(objCustomDataDirectoryToDown.Item1 + 1, objCustomDataDirectoryToDown.Item2);
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryNames));
            PopulateCustomDataDirectoryTreeView();
        }

        private void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            int intIndex = objNode.Index;
            _lstCharacterCustomDataDirectoryInfos[intIndex] = new Tuple<object, bool>(_lstCharacterCustomDataDirectoryInfos[intIndex].Item1, objNode.Checked);
            if (objNode.Tag is CustomDataDirectoryInfo objCustomDataDirectoryInfo
                && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(objCustomDataDirectoryInfo.Name, out var objValue))
            {
                _objCharacterOptions.CustomDataDirectoryNames[objCustomDataDirectoryInfo.Name] = new Tuple<int, bool>(objValue.Item1, objNode.Checked);
                _objCharacterOptions.RecalculateEnabledCustomDataDirectories();
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryNames));
            }
            else if (objNode.Tag is string strCustomDataDirectoryKey
                     && _objCharacterOptions.CustomDataDirectoryNames.TryGetValue(strCustomDataDirectoryKey, out objValue))
            {
                _objCharacterOptions.CustomDataDirectoryNames[strCustomDataDirectoryKey] = new Tuple<int, bool>(objValue.Item1, objNode.Checked);
                _objCharacterOptions.RecalculateEnabledCustomDataDirectories();
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryNames));
            }
        }

        private void txtPriorities_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar)
                        && e.KeyChar != 'A' && e.KeyChar != 'B' && e.KeyChar != 'C' && e.KeyChar != 'D' && e.KeyChar != 'E'
                        && e.KeyChar != 'a' && e.KeyChar != 'b' && e.KeyChar != 'c' && e.KeyChar != 'd' && e.KeyChar != 'e';
            switch (e.KeyChar)
            {
                case 'a':
                    e.KeyChar = 'A';
                    break;
                case 'b':
                    e.KeyChar = 'B';
                    break;
                case 'c':
                    e.KeyChar = 'C';
                    break;
                case 'd':
                    e.KeyChar = 'D';
                    break;
                case 'e':
                    e.KeyChar = 'E';
                    break;
            }
        }

        private void txtPriorities_TextChanged(object sender, EventArgs e)
        {
            txtPriorities.ForeColor = txtPriorities.Text.Length == 5 ? SystemColors.WindowText : Color.Red;
        }

        private void txtContactPoints_TextChanged(object sender, EventArgs e)
        {
            string strExpression = txtContactPoints.Text;
            if (!string.IsNullOrEmpty(strExpression))
            {
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    strExpression = strExpression
                        .Replace('{' + strCharAttributeName + '}', "0")
                        .Replace('{' + strCharAttributeName + "Unaug}", "0")
                        .Replace('{' + strCharAttributeName + "Base}", "0");
                }
                CommonFunctions.EvaluateInvariantXPath(strExpression, out bool blnSuccess);
                if (!blnSuccess)
                    txtContactPoints.ForeColor = Color.Red;
            }
            txtContactPoints.ForeColor = SystemColors.WindowText;
        }

        private void txtKnowledgePoints_TextChanged(object sender, EventArgs e)
        {
            string strExpression = txtKnowledgePoints.Text;
            if (!string.IsNullOrEmpty(strExpression))
            {
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    strExpression = strExpression
                        .Replace('{' + strCharAttributeName + '}', "0")
                        .Replace('{' + strCharAttributeName + "Unaug}", "0")
                        .Replace('{' + strCharAttributeName + "Base}", "0");
                }
                CommonFunctions.EvaluateInvariantXPath(strExpression, out bool blnSuccess);
                if (!blnSuccess)
                    txtKnowledgePoints.ForeColor = Color.Red;
            }
            txtKnowledgePoints.ForeColor = SystemColors.WindowText;
        }

        private void chkGrade_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox chkGrade))
                return;

            string strGrade = chkGrade.Tag.ToString();
            if (chkGrade.Checked)
            {
                if (_objCharacterOptions.BannedWareGrades.Contains(strGrade))
                {
                    _objCharacterOptions.BannedWareGrades.Remove(strGrade);
                    _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.BannedWareGrades));
                }
            }
            else if (!_objCharacterOptions.BannedWareGrades.Contains(strGrade))
            {
                _objCharacterOptions.BannedWareGrades.Add(strGrade);
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.BannedWareGrades));
            }
        }
        #endregion

        #region Methods

        private void PopulateSourcebookTreeView()
        {
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths);

            // Put the Sourcebooks into a List so they can first be sorted.
            object objOldSelected = treSourcebook.SelectedNode?.Tag;
            treSourcebook.Nodes.Clear();
            _setPermanentSourcebooks.Clear();
            using (XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book"))
            {
                if(objXmlBookList != null)
                {
                    foreach(XmlNode objXmlBook in objXmlBookList)
                    {
                        if(objXmlBook["hide"] != null)
                            continue;
                        string strCode = objXmlBook["code"]?.InnerText;
                        bool blnChecked = _objCharacterOptions.Books.Contains(strCode);
                        if (objXmlBook["permanent"] != null)
                        {
                            _setPermanentSourcebooks.Add(strCode);
                            _objCharacterOptions.Books.Add(strCode);
                            blnChecked = true;
                        }
                        TreeNode objNode = new TreeNode
                        {
                            Text = objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ?? string.Empty,
                            Tag = strCode,
                            Checked = blnChecked
                        };
                        treSourcebook.Nodes.Add(objNode);
                    }
                }
            }

            treSourcebook.Sort();
            if (objOldSelected != null)
                treSourcebook.SelectedNode = treSourcebook.FindNodeByTag(objOldSelected);
        }

        private void PopulateCustomDataDirectoryTreeView()
        {
            object objOldSelected = treCustomDataDirectories.SelectedNode?.Tag;
            if(_lstCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (Tuple<object, bool> objCustomDataDirectory in _lstCharacterCustomDataDirectoryInfos)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Tag = objCustomDataDirectory.Item1,
                        Checked = objCustomDataDirectory.Item2
                    };
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for(int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataDirectories.Nodes[i];
                    Tuple<object, bool> objCustomDataDirectory = _lstCharacterCustomDataDirectoryInfos[i];
                    objNode.Tag = objCustomDataDirectory.Item1;
                    objNode.Checked = objCustomDataDirectory.Item2;
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                }
            }

            if(objOldSelected != null)
                treCustomDataDirectories.SelectedNode = treCustomDataDirectories.FindNodeByTag(objOldSelected);
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            PopulateSourcebookTreeView();
            PopulatePriorityTableList();
            PopulateLimbCountList();
            PopulateAllowedGrades();
        }

        private void PopulatePriorityTableList()
        {
            List<ListItem> lstPriorityTables = new List<ListItem>();

            using (XmlNodeList objXmlNodeList = XmlManager.Load("priorities.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .SelectNodes("/chummer/prioritytables/prioritytable"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strName = objXmlNode.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                            lstPriorityTables.Add(new ListItem(objXmlNode.InnerText, objXmlNode.Attributes?["translate"]?.InnerText ?? strName));
                    }
                }
            }

            string strOldSelected = cboPriorityTable.SelectedValue?.ToString();

            cboPriorityTable.BeginUpdate();
            cboPriorityTable.DataSource = null;
            cboPriorityTable.ValueMember = nameof(ListItem.Value);
            cboPriorityTable.DisplayMember = nameof(ListItem.Name);
            cboPriorityTable.DataSource = lstPriorityTables;

            if (!string.IsNullOrEmpty(strOldSelected))
                cboPriorityTable.SelectedValue = strOldSelected;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedValue = _objReferenceCharacterOptions.PriorityTable;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedIndex = 0;

            cboPriorityTable.EndUpdate();
        }

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            using (XmlNodeList objXmlNodeList = XmlManager.Load("options.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .SelectNodes("/chummer/limbcounts/limb"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strExclude = objXmlNode["exclude"]?.InnerText ?? string.Empty;
                        if (!string.IsNullOrEmpty(strExclude))
                            strExclude = '<' + strExclude;
                        lstLimbCount.Add(new ListItem(objXmlNode["limbcount"]?.InnerText + strExclude, objXmlNode["translate"]?.InnerText ?? objXmlNode["name"]?.InnerText ?? string.Empty));
                    }
                }
            }

            string strOldSelected = cboLimbCount.SelectedValue?.ToString();

            _blnSkipLimbCountUpdate = true;
            cboLimbCount.BeginUpdate();
            cboLimbCount.ValueMember = nameof(ListItem.Value);
            cboLimbCount.DisplayMember = nameof(ListItem.Name);
            cboLimbCount.DataSource = lstLimbCount;

            if (!string.IsNullOrEmpty(strOldSelected))
            {
                cboLimbCount.SelectedValue = strOldSelected;
                if (cboLimbCount.SelectedIndex == -1 && lstLimbCount.Count > 0)
                {
                    string strLimbSlot = _objCharacterOptions.LimbCount.ToString(GlobalOptions.InvariantCultureInfo);
                    if (!string.IsNullOrEmpty(_objCharacterOptions.ExcludeLimbSlot))
                        strLimbSlot += '<' + _objCharacterOptions.ExcludeLimbSlot;
                    cboLimbCount.SelectedValue = strLimbSlot;
                }
                if (cboLimbCount.SelectedIndex == -1 && lstLimbCount.Count > 0)
                    cboLimbCount.SelectedIndex = 0;
            }

            cboLimbCount.EndUpdate();
            _blnSkipLimbCountUpdate = false;
            if (cboLimbCount.SelectedValue.ToString() != strOldSelected)
                cboLimbCount_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void PopulateAllowedGrades()
        {
            flpAllowedCyberwareGrades.SuspendLayout();
            flpAllowedCyberwareGrades.Controls.Clear();

            List<ListItem> lstGrades = new List<ListItem>();

            using (XmlNodeList objXmlNodeList = XmlManager.Load("bioware.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .SelectNodes("/chummer/grades/grade[not(hide)]"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strName = objXmlNode["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName) && strName != "None")
                        {
                            string strBook = objXmlNode["source"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (lstGrades.Any(x => strName.Contains(x.Name)))
                                continue;
                            ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Name.Contains(strName));
                            if (objExistingCoveredGrade != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(objXmlNode.InnerText, objXmlNode["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }
            using (XmlNodeList objXmlNodeList = XmlManager.Load("cyberware.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .SelectNodes("/chummer/grades/grade[not(hide)]"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strName = objXmlNode["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName) && strName != "None" && lstGrades.All(x => x.Value.ToString() != strName))
                        {
                            string strBook = objXmlNode["source"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                                continue;
                            if (lstGrades.Any(x => strName.Contains(x.Name)))
                                continue;
                            ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Name.Contains(strName));
                            if (objExistingCoveredGrade != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(objXmlNode.InnerText, objXmlNode["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }

            foreach (ListItem objGrade in lstGrades)
            {
                CheckBox chkGrade = new CheckBox
                {
                    UseVisualStyleBackColor = true,
                    Text = objGrade.Name,
                    Tag = objGrade.Value,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Checked = !_objCharacterOptions.BannedWareGrades.Contains(objGrade.Value.ToString())
                };
                chkGrade.CheckedChanged += chkGrade_CheckedChanged;
                flpAllowedCyberwareGrades.Controls.Add(chkGrade);
            }

            flpAllowedCyberwareGrades.ResumeLayout();
        }

        private void SetToolTips()
        {
            const int width = 100;
            chkUnarmedSkillImprovements.SetToolTip(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements").WordWrap(width));
            chkIgnoreArt.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreArt").WordWrap(width));
            chkIgnoreComplexFormLimit.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreComplexFormLimit").WordWrap(width));
            chkCyberlegMovement.SetToolTip(LanguageManager.GetString("Tip_OptionsCyberlegMovement").WordWrap(width));
            chkDontDoubleQualityPurchases.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases").WordWrap(width));
            chkDontDoubleQualityRefunds.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds").WordWrap(width));
            chkStrictSkillGroups.SetToolTip(LanguageManager.GetString("Tip_OptionStrictSkillGroups").WordWrap(width));
            chkAllowInitiation.SetToolTip(LanguageManager.GetString("Tip_OptionsAllowInitiation").WordWrap(width));
            chkUseCalculatedPublicAwareness.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness").WordWrap(width));
        }

        private void SetupDataBindings()
        {
            cmdDelete.DoOneWayNegatableDatabinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.BuiltInOption));

            cboBuildMethod.DoDatabinding("SelectedValue", _objCharacterOptions, nameof(CharacterOptions.BuildMethod));
            lblPriorityTable.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodUsesPriorityTables));
            cboPriorityTable.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodUsesPriorityTables));
            cboPriorityTable.DoDatabinding("SelectedValue", _objCharacterOptions, nameof(CharacterOptions.PriorityTable));
            lblPriorities.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsPriority));
            txtPriorities.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsPriority));
            txtPriorities.DoDatabinding("Text", _objCharacterOptions, nameof(CharacterOptions.PriorityArray));
            lblSumToTen.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsSumtoTen));
            nudSumToTen.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsSumtoTen));
            nudSumToTen.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.SumtoTen));
            nudStartingKarma.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.BuildKarma));
            nudMaxNuyenKarma.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.NuyenMaximumBP));
            nudMaxAvail.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.MaximumAvailability));
            nudQualityKarmaLimit.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.QualityKarmaLimit));
            txtContactPoints.DoDatabinding("Text", _objCharacterOptions, nameof(CharacterOptions.ContactPointsExpression));
            txtKnowledgePoints.DoDatabinding("Text", _objCharacterOptions, nameof(CharacterOptions.KnowledgePointsExpression));

            chkPrintExpenses.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrintExpenses));
            chkPrintFreeExpenses.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.PrintExpenses));
            chkPrintFreeExpenses.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrintFreeExpenses));
            chkPrintNotes.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrintNotes));
            chkPrintSkillsWithZeroRating.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrintSkillsWithZeroRating));

            chkEnforceCapacity.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.EnforceCapacity));
            chkLicenseEachRestrictedItem.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.LicenseRestricted));
            chkReverseAttributePriorityOrder.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ReverseAttributePriorityOrder));
            chkDronemods.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneMods));
            chkDronemodsMaximumPilot.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneModsMaximumPilot));
            chkRestrictRecoil.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RestrictRecoil));
            chkStrictSkillGroups.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.StrictSkillGroupsInCreateMode));
            chkAllowPointBuySpecializationsOnKarmaSkills.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowPointBuySpecializationsOnKarmaSkills));
            chkAllowFreeGrids.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowFreeGrids));
            chkEnemyKarmaQualityLimit.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.EnemyKarmaQualityLimit));

            chkDontUseCyberlimbCalculation.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontUseCyberlimbCalculation));
            chkCyberlegMovement.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CyberlegMovement));
            chkCyberlimbAttributeBonusCap.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCap));
            chkRedlinerLimbsSkull.DoNegatableDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesSkull));
            chkRedlinerLimbsTorso.DoNegatableDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesTorso));
            chkRedlinerLimbsArms.DoNegatableDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesArms));
            chkRedlinerLimbsLegs.DoNegatableDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesLegs));

            nudNuyenDecimalsMaximum.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.MaxNuyenDecimals));
            nudNuyenDecimalsMinimum.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.MinNuyenDecimals));
            nudEssenceDecimals.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.EssenceDecimals));
            chkDontRoundEssenceInternally.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontRoundEssenceInternally));

            chkMoreLethalGameplay.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MoreLethalGameplay));
            chkNoArmorEncumbrance.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.NoArmorEncumbrance));
            chkIgnoreArt.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IgnoreArt));
            chkIgnoreComplexFormLimit.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IgnoreComplexFormLimit));
            chkUnarmedSkillImprovements.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UnarmedImprovementsApplyToWeapons));
            chkMysAdPp.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MysAdeptAllowPPCareer));
            chkMysAdPp.DoOneWayNegatableDatabinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkPrioritySpellsAsAdeptPowers.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrioritySpellsAsAdeptPowers));
            chkPrioritySpellsAsAdeptPowers.DoOneWayNegatableDatabinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoOneWayNegatableDatabinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttributeEnabled));
            chkUsePointsOnBrokenGroups.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UsePointsOnBrokenGroups));
            chkSpecialKarmaCost.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.SpecialKarmaCostBasedOnShownValue));
            chkUseCalculatedPublicAwareness.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UseCalculatedPublicAwareness));
            chkAlternateMetatypeAttributeKarma.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AlternateMetatypeAttributeKarma));
            chkCompensateSkillGroupKarmaDifference.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CompensateSkillGroupKarmaDifference));
            chkFreeMartialArtSpecialization.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.FreeMartialArtSpecialization));
            chkIncreasedImprovedAbilityModifier.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IncreasedImprovedAbilityMultiplier));
            chkAllowTechnomancerSchooling.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowTechnomancerSchooling));
            chkAllowSkillRegrouping.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowSkillRegrouping));
            chkDontDoubleQualityPurchases.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontDoubleQualityPurchases));
            chkDontDoubleQualityRefunds.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontDoubleQualityRefunds));
            chkDroneArmorMultiplier.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplier));
            chkESSLossReducesMaximumOnly.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ESSLossReducesMaximumOnly));
            chkExceedNegativeQualities.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualitiesLimit));
            chkExceedPositiveQualities.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualitiesCostDoubled));
            chkExtendAnyDetectionSpell.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExtendAnyDetectionSpell));
            chkAllowCyberwareESSDiscounts.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowCyberwareESSDiscounts));
            chkAllowInitiation.DoDatabinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowInitiationInCreateMode));

            // Karma options.
            nudMetatypeCostsKarmaMultiplier.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.MetatypeCostsKarmaMultiplier));
            nudKarmaNuyenPer.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.NuyenPerBP));
            nudKarmaAttribute.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaAttribute));
            nudKarmaSpecialization.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaQuality));
            nudKarmaSpecialization.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpecialization));
            nudKarmaKnowledgeSpecialization.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaKnowledgeSpecialization));
            nudKarmaNewKnowledgeSkill.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewKnowledgeSkill));
            nudKarmaNewActiveSkill.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewActiveSkill));
            nudKarmaNewSkillGroup.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewSkillGroup));
            nudKarmaImproveKnowledgeSkill.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveKnowledgeSkill));
            nudKarmaImproveActiveSkill.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveActiveSkill));
            nudKarmaImproveSkillGroup.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveSkillGroup));
            nudKarmaSpell.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpell));
            nudKarmaNewComplexForm.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewComplexForm));
            nudKarmaImproveComplexForm.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveComplexForm));
            nudKarmaNewAIProgram.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIProgram));
            nudKarmaNewAIAdvancedProgram.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIAdvancedProgram));
            nudKarmaMetamagic.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMetamagic));
            nudKarmaContact.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaContact));
            nudKarmaEnemy.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaEnemy));
            nudKarmaCarryover.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCarryover));
            nudKarmaSpirit.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpirit));
            nudKarmaManeuver.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaManeuver));
            nudKarmaInitiation.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiation));
            nudKarmaInitiationFlat.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiationFlat));
            nudKarmaComplexFormOption.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaComplexFormOption));
            nudKarmaComplexFormSkillsoft.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaComplexFormSkillsoft));
            nudKarmaJoinGroup.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaJoinGroup));
            nudKarmaLeaveGroup.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaLeaveGroup));
            nudKarmaMysticAdeptPowerPoint.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMysticAdeptPowerPoint));

            // Focus costs
            nudKarmaAlchemicalFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaAlchemicalFocus));
            nudKarmaBanishingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaBanishingFocus));
            nudKarmaBindingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaBindingFocus));
            nudKarmaCenteringFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCenteringFocus));
            nudKarmaCounterspellingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCounterspellingFocus));
            nudKarmaDisenchantingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaDisenchantingFocus));
            nudKarmaFlexibleSignatureFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaFlexibleSignatureFocus));
            nudKarmaMaskingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMaskingFocus));
            nudKarmaPowerFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaPowerFocus));
            nudKarmaQiFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaQiFocus));
            nudKarmaRitualSpellcastingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaRitualSpellcastingFocus));
            nudKarmaSpellcastingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpellcastingFocus));
            nudKarmaSpellShapingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpellShapingFocus));
            nudKarmaSummoningFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSummoningFocus));
            nudKarmaSustainingFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSustainingFocus));
            nudKarmaWeaponFocus.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaWeaponFocus));

            _objCharacterOptions.PropertyChanged += OptionsChanged;
        }

        private void PopulateSettingsList()
        {
            string strOldSelected = string.Empty;
            if (!_blnLoading)
                strOldSelected = cboSetting.SelectedValue?.ToString();
            List<ListItem> lstSettings = new List<ListItem>();
            foreach (KeyValuePair<string, CharacterOptions> kvpCharacterOptionsEntry in OptionsManager.LoadedCharacterOptions)
            {
                lstSettings.Add(new ListItem(kvpCharacterOptionsEntry.Key, kvpCharacterOptionsEntry.Value.DisplayName));
                if (_blnLoading && _objReferenceCharacterOptions == kvpCharacterOptionsEntry.Value)
                    strOldSelected = kvpCharacterOptionsEntry.Key;
            }

            cboSetting.BeginUpdate();
            cboSetting.DataSource = null;
            cboSetting.ValueMember = nameof(ListItem.Value);
            cboSetting.DisplayMember = nameof(ListItem.Name);
            cboSetting.DataSource = lstSettings;
            if (!string.IsNullOrEmpty(strOldSelected))
                cboSetting.SelectedValue = strOldSelected;
            if (cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedValue = cboSetting.FindStringExact(GlobalOptions.DefaultGameplayOption);
            if (cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;
            cboSetting.EndUpdate();
        }

        private void OptionsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_blnLoading)
            {
                _blnDirty = !_objCharacterOptions.Equals(_objReferenceCharacterOptions);
                cmdSaveAs.Enabled = _blnDirty && IsAllTextBoxesLegal;
                cmdSave.Enabled = cmdSaveAs.Enabled && !_objCharacterOptions.BuiltInOption;
                if (e.PropertyName == nameof(CharacterOptions.EnabledCustomDataDirectoryPaths))
                    PopulateOptions();
                if (cmdSave.Enabled)
                {
                    if (_objReferenceCharacterOptions.BuildMethod != _objCharacterOptions.BuildMethod
                        && Program.MainForm.OpenCharacters.Any(x => !x.Created && x.Options == _objReferenceCharacterOptions))
                        cmdSave.Enabled = false;
                }
            }
            else if (e.PropertyName == nameof(CharacterOptions.BuiltInOption))
            {
                cmdSave.Enabled = cmdSaveAs.Enabled
                                  && !_objCharacterOptions.BuiltInOption
                                  && (_objReferenceCharacterOptions.BuildMethod == _objCharacterOptions.BuildMethod
                                      || Program.MainForm.OpenCharacters.All(x => x.Created || x.Options != _objReferenceCharacterOptions));
            }
            else if (e.PropertyName == nameof(CharacterOptions.PriorityArray)
                     || e.PropertyName == nameof(CharacterOptions.BuildMethod))
            {
                cmdSaveAs.Enabled = _blnDirty && IsAllTextBoxesLegal;
                cmdSave.Enabled = cmdSaveAs.Enabled
                                  && !_objCharacterOptions.BuiltInOption
                                  && (_objReferenceCharacterOptions.BuildMethod == _objCharacterOptions.BuildMethod
                                      || Program.MainForm.OpenCharacters.All(x => x.Created || x.Options != _objReferenceCharacterOptions));
            }
        }

        private bool IsAllTextBoxesLegal
        {
            get
            {
                if (_objCharacterOptions.BuildMethod == CharacterBuildMethod.Priority && _objCharacterOptions.PriorityArray.Length != 5)
                    return false;

                string strContactPointsExpression = _objCharacterOptions.ContactPointsExpression;
                string strKnowledgePointsExpression = _objCharacterOptions.KnowledgePointsExpression;
                if (string.IsNullOrEmpty(strContactPointsExpression) && !string.IsNullOrEmpty(strKnowledgePointsExpression))
                    return true;
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    if (!string.IsNullOrEmpty(strContactPointsExpression))
                        strContactPointsExpression = strContactPointsExpression
                            .Replace('{' + strCharAttributeName + '}', "0")
                            .Replace('{' + strCharAttributeName + "Unaug}", "0")
                            .Replace('{' + strCharAttributeName + "Base}", "0");
                    if (!string.IsNullOrEmpty(strKnowledgePointsExpression))
                        strKnowledgePointsExpression = strKnowledgePointsExpression
                            .Replace('{' + strCharAttributeName + '}', "0")
                            .Replace('{' + strCharAttributeName + "Unaug}", "0")
                            .Replace('{' + strCharAttributeName + "Base}", "0");
                }

                if (!string.IsNullOrEmpty(strContactPointsExpression))
                {
                    CommonFunctions.EvaluateInvariantXPath(strContactPointsExpression, out bool blnSuccess);
                    if (!blnSuccess)
                        return false;
                }
                if (!string.IsNullOrEmpty(strKnowledgePointsExpression))
                {
                    CommonFunctions.EvaluateInvariantXPath(strKnowledgePointsExpression, out bool blnSuccess);
                    if (!blnSuccess)
                        return false;
                }

                return true;
            }
        }

        #endregion
    }
}
