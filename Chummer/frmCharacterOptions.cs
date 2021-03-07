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
        private bool _blnWasRenamed;
        private bool _blnIsLayoutSuspended = true;
        private readonly HashSet<string> _setPermanentSourcebooks = new HashSet<string>();

        #region Form Events
        public frmCharacterOptions(CharacterOptions objExistingOptions = null)
        {
            InitializeComponent();
            this.TranslateWinForm();
            _objReferenceCharacterOptions = objExistingOptions ?? OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];
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
            cboBuildMethod.DataSource = null;
            cboBuildMethod.DataSource = lstBuildMethods;
            if (_blnLoading)
            {
                cboBuildMethod.ValueMember = nameof(ListItem.Value);
                cboBuildMethod.DisplayMember = nameof(ListItem.Name);
            }
            cboBuildMethod.EndUpdate();

            PopulateOptions();
            SetupDataBindings();

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

            IsDirty = false;
            cmdSaveAs.Enabled = false;
            cmdSave.Enabled = false;

            _blnLoading = false;
            _blnIsLayoutSuspended = false;
        }
        #endregion

        #region Control Events
        private void cmdGlobalOptionsCustomData_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                using (frmOptions frmOptions = new frmOptions("tabCustomDataDirectories"))
                    frmOptions.ShowDialog(this);
        }

        private void cmdRename_Click(object sender, EventArgs e)
        {
            using (frmSelectText frmSelectName = new frmSelectText
            {
                DefaultString = _objCharacterOptions.Name,
                Description = LanguageManager.GetString("Message_CharacterOptions_SettingRename")
            })
            {
                frmSelectName.ShowDialog(this);
                if (frmSelectName.DialogResult != DialogResult.OK)
                    return;
                _objCharacterOptions.Name = frmSelectName.SelectedValue;
            }

            using (new CursorWait(this))
            {
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (cboSetting.SelectedItem != null)
                {
                    ListItem objCurrentListItem = (ListItem) cboSetting.SelectedItem;
                    ListItem objNewListItem = new ListItem(objCurrentListItem.Value, _objCharacterOptions.DisplayName);
                    _blnLoading = true;
                    cboSetting.BeginUpdate();
                    cboSetting.Items[cboSetting.Items.IndexOf(cboSetting.SelectedItem)] = objNewListItem;
                    cboSetting.SelectedItem = objNewListItem;
                    cboSetting.EndUpdate();
                    _blnLoading = false;
                }

                _blnWasRenamed = true;
                IsDirty = true;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to delete this setting
            if (Program.MainForm.ShowMessageBox(
                string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_CharacterOptions_ConfirmDelete"),
                    _objReferenceCharacterOptions.Name),
                LanguageManager.GetString("MessageTitle_Options_ConfirmDelete"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                File.Delete(_objReferenceCharacterOptions.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                return;
            }

            using (new CursorWait(this))
            {
                OptionsManager.LoadedCharacterOptions.Remove(_objReferenceCharacterOptions.FileName);
                KeyValuePair<string, CharacterOptions> kvpReplacementOption =
                    OptionsManager.LoadedCharacterOptions.First(x => x.Value.BuiltInOption
                                                                     && x.Value.BuildMethod ==
                                                                     _objReferenceCharacterOptions.BuildMethod);
                foreach (Character objCharacter in Program.MainForm.OpenCharacters.Where(x =>
                    x.CharacterOptionsKey == _objReferenceCharacterOptions.FileName))
                    objCharacter.CharacterOptionsKey = kvpReplacementOption.Key;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterOptions = kvpReplacementOption.Value;
                _objCharacterOptions.CopyValues(_objReferenceCharacterOptions);
                IsDirty = false;
                PopulateSettingsList();
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdSaveAs_Click(object sender, EventArgs e)
        {
            string strSelectedName;
            do
            {
                using (frmSelectText frmSelectName = new frmSelectText
                {
                    DefaultString = _objCharacterOptions.BuiltInOption ? string.Empty : _objCharacterOptions.FileName.TrimEndOnce(".xml"),
                    PreventFileNameCharErrors = true,
                    Description = LanguageManager.GetString("Message_CharacterOptions_SelectSettingFileName")
                })
                {
                    frmSelectName.ShowDialog(this);
                    if (frmSelectName.DialogResult != DialogResult.OK)
                        return;
                    strSelectedName = frmSelectName.SelectedValue;
                    if (!strSelectedName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        strSelectedName += ".xml";
                }

                if (OptionsManager.LoadedCharacterOptions.Any(x => x.Value.FileName == strSelectedName))
                {
                    // TODO: Turn this into a prompt to overwrite (which also needs to consider build method being overwritten for a character)
                    Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_CharacterOptions_DuplicateSettingFileName"), strSelectedName),
                        LanguageManager.GetString("MessageTitle_CharacterOptions_DuplicateSettingFileName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    strSelectedName = string.Empty;
                }
            } while (string.IsNullOrEmpty(strSelectedName));

            using (new CursorWait(this))
            {
                if (!_objCharacterOptions.Save(strSelectedName))
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                CharacterOptions objNewCharacterOptions = new CharacterOptions();
                objNewCharacterOptions.CopyValues(_objCharacterOptions);
                OptionsManager.LoadedCharacterOptions.Add(
                    Path.Combine(Utils.GetStartupPath, "settings", objNewCharacterOptions.FileName),
                    objNewCharacterOptions);
                _objReferenceCharacterOptions = objNewCharacterOptions;
                cmdSaveAs.Enabled = false;
                cmdSave.Enabled = false;
                IsDirty = false;
                PopulateSettingsList();
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                if (!_objCharacterOptions.Save())
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterOptions.CopyValues(_objCharacterOptions);
                cmdSaveAs.Enabled = false;
                cmdSave.Enabled = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedFile = cboSetting.SelectedValue?.ToString();
            if(string.IsNullOrEmpty(strSelectedFile) || !OptionsManager.LoadedCharacterOptions.TryGetValue(strSelectedFile, out CharacterOptions objNewOption))
                return;

            if (IsDirty)
            {
                string text = LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty");
                string caption = LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty");

                if (Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            using (new CursorWait(this))
            {
                _blnLoading = true;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (_blnWasRenamed && cboSetting.SelectedItem != null)
                {
                    ListItem objCurrentListItem = (ListItem) cboSetting.SelectedItem;
                    ListItem objNewListItem =
                        new ListItem(objCurrentListItem.Value, _objReferenceCharacterOptions.DisplayName);
                    cboSetting.BeginUpdate();
                    cboSetting.Items[cboSetting.Items.IndexOf(cboSetting.SelectedItem)] = objNewListItem;
                    cboSetting.SelectedItem = objNewListItem;
                    cboSetting.EndUpdate();
                }

                _objReferenceCharacterOptions = objNewOption;
                _objCharacterOptions.CopyValues(objNewOption);
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdRestoreDefaults_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if(Program.MainForm.ShowMessageBox(
                LanguageManager.GetString("Message_Options_RestoreDefaults"),
                LanguageManager.GetString("MessageTitle_Options_RestoreDefaults"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            
            using (new CursorWait(this))
            {
                _blnLoading = true;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (_blnWasRenamed && cboSetting.SelectedItem != null)
                {
                    ListItem objCurrentListItem = (ListItem) cboSetting.SelectedItem;
                    ListItem objNewListItem =
                        new ListItem(objCurrentListItem.Value, _objReferenceCharacterOptions.DisplayName);
                    cboSetting.BeginUpdate();
                    cboSetting.Items[cboSetting.Items.IndexOf(cboSetting.SelectedItem)] = objNewListItem;
                    cboSetting.SelectedItem = objNewListItem;
                    cboSetting.EndUpdate();
                }

                _objCharacterOptions.CopyValues(_objReferenceCharacterOptions);
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
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

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (IsDirty)
            {
                if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty"),
                    LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                DialogResult = DialogResult.Cancel;
            }
            else
                DialogResult = DialogResult.OK;
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
            treSourcebook.BeginUpdate();
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
            treSourcebook.EndUpdate();
        }

        private void PopulateCustomDataDirectoryTreeView()
        {
            object objOldSelected = treCustomDataDirectories.SelectedNode?.Tag;
            treCustomDataDirectories.BeginUpdate();
            if (_lstCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
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
            treCustomDataDirectories.EndUpdate();
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            bool blnDoResumeLayout = !_blnIsLayoutSuspended;
            if (blnDoResumeLayout)
            {
                _blnIsLayoutSuspended = true;
                SuspendLayout();
            }
            PopulateSourcebookTreeView();
            PopulatePriorityTableList();
            PopulateLimbCountList();
            PopulateAllowedGrades();
            if (blnDoResumeLayout)
            {
                _blnIsLayoutSuspended = false;
                ResumeLayout();
            }
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
            cboPriorityTable.DataSource = lstPriorityTables;
            if (_blnLoading)
            {
                cboPriorityTable.ValueMember = nameof(ListItem.Value);
                cboPriorityTable.DisplayMember = nameof(ListItem.Name);
            }

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

            string strLimbSlot = _objCharacterOptions.LimbCount.ToString(GlobalOptions.InvariantCultureInfo);
            if (!string.IsNullOrEmpty(_objCharacterOptions.ExcludeLimbSlot))
                strLimbSlot += '<' + _objCharacterOptions.ExcludeLimbSlot;

            _blnSkipLimbCountUpdate = true;
            cboLimbCount.BeginUpdate();
            cboLimbCount.DataSource = null;
            cboLimbCount.DataSource = lstLimbCount;
            if (_blnLoading)
            {
                cboLimbCount.ValueMember = nameof(ListItem.Value);
                cboLimbCount.DisplayMember = nameof(ListItem.Name);
            }

            if (!string.IsNullOrEmpty(strLimbSlot))
                cboLimbCount.SelectedValue = strLimbSlot;
            if (cboLimbCount.SelectedIndex == -1 && lstLimbCount.Count > 0)
                cboLimbCount.SelectedIndex = 0;

            cboLimbCount.EndUpdate();
            _blnSkipLimbCountUpdate = false;
        }

        private void PopulateAllowedGrades()
        {
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
                            if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(strName, objXmlNode["translate"]?.InnerText ?? strName));
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
                            if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                                continue;
                            ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                            if (objExistingCoveredGrade.Value != null)
                                lstGrades.Remove(objExistingCoveredGrade);
                            lstGrades.Add(new ListItem(strName, objXmlNode["translate"]?.InnerText ?? strName));
                        }
                    }
                }
            }

            flpAllowedCyberwareGrades.SuspendLayout();
            flpAllowedCyberwareGrades.Controls.Clear();
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
            chkUnarmedSkillImprovements.SetToolTip(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements").WordWrap());
            chkIgnoreArt.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreArt").WordWrap());
            chkIgnoreComplexFormLimit.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreComplexFormLimit").WordWrap());
            chkCyberlegMovement.SetToolTip(LanguageManager.GetString("Tip_OptionsCyberlegMovement").WordWrap());
            chkDontDoubleQualityPurchases.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases").WordWrap());
            chkDontDoubleQualityRefunds.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds").WordWrap());
            chkStrictSkillGroups.SetToolTip(LanguageManager.GetString("Tip_OptionStrictSkillGroups").WordWrap());
            chkAllowInitiation.SetToolTip(LanguageManager.GetString("Tip_OptionsAllowInitiation").WordWrap());
            chkUseCalculatedPublicAwareness.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness").WordWrap());
        }

        private void SetupDataBindings()
        {
            cmdRename.DoOneWayNegatableDatabinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.BuiltInOption));
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
            chkMysAdeptSecondMAGAttribute.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttributeEnabled));
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
            nudKarmaQuality.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaQuality));
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
            nudKarmaNewAIProgram.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIProgram));
            nudKarmaNewAIAdvancedProgram.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIAdvancedProgram));
            nudKarmaMetamagic.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMetamagic));
            nudKarmaContact.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaContact));
            nudKarmaEnemy.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaEnemy));
            nudKarmaCarryover.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCarryover));
            nudKarmaSpirit.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpirit));
            nudKarmaTechnique.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaTechnique));
            nudKarmaInitiation.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiation));
            nudKarmaInitiationFlat.DoDatabinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiationFlat));
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
            cboSetting.DataSource = lstSettings;
            if (_blnLoading)
            {
                cboSetting.ValueMember = nameof(ListItem.Value);
                cboSetting.DisplayMember = nameof(ListItem.Name);
            }
            if (!string.IsNullOrEmpty(strOldSelected))
                cboSetting.SelectedValue = strOldSelected;
            if (cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedValue = cboSetting.FindStringExact(GlobalOptions.DefaultCharacterOption);
            if (cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;
            cboSetting.EndUpdate();
        }

        private void OptionsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_blnLoading)
            {
                IsDirty = !_objCharacterOptions.Equals(_objReferenceCharacterOptions);
                cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
                cmdSave.Enabled = cmdSaveAs.Enabled && !_objCharacterOptions.BuiltInOption;
                if (e.PropertyName == nameof(CharacterOptions.EnabledCustomDataDirectoryPaths))
                    PopulateOptions();
                if (cmdSave.Enabled && _objReferenceCharacterOptions.BuildMethod != _objCharacterOptions.BuildMethod && Program.MainForm.OpenCharacters.Any(x => !x.Created && x.Options == _objReferenceCharacterOptions))
                {
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
                cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
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

        private bool IsDirty
        {
            get => _blnDirty;
            set
            {
                if (_blnDirty != value)
                {
                    _blnDirty = value;
                    cmdOK.Text = LanguageManager.GetString(value ? "String_Cancel" : "String_OK");
                    if (!value)
                        _blnWasRenamed = false;
                }
            }
        }
        #endregion
    }
}
