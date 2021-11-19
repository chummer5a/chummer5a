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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    public partial class frmCharacterSettings : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private readonly CharacterSettings _objCharacterSettings;
        private CharacterSettings _objReferenceCharacterSettings;
        private readonly List<ListItem> _lstSettings = new List<ListItem>();

        // List of custom data directory infos on the character, in load order. If the character has a directory name for which we have no info, key will be a string instead of an info
        private readonly TypedOrderedDictionary<object, bool> _dicCharacterCustomDataDirectoryInfos = new TypedOrderedDictionary<object, bool>();

        private bool _blnLoading = true;
        private bool _blnSkipLimbCountUpdate;
        private bool _blnDirty;
        private bool _blnSourcebookToggle = true;
        private bool _blnWasRenamed;
        private bool _blnIsLayoutSuspended = true;
        private bool _blnForceMasterIndexRepopulateOnClose;

        // Used to revert to old selected setting if user cancels out of selecting a different one
        private int _intOldSelectedSettingIndex = -1;

        private readonly HashSet<string> _setPermanentSourcebooks = new HashSet<string>();

        #region Form Events

        public frmCharacterSettings(CharacterSettings objExistingSettings = null)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objReferenceCharacterSettings = objExistingSettings ?? SettingsManager.LoadedCharacterSettings[GlobalSettings.DefaultCharacterSetting];
            _objCharacterSettings = new CharacterSettings(_objReferenceCharacterSettings);
            RebuildCustomDataDirectoryInfos();
        }

        private void frmCharacterSettings_Load(object sender, EventArgs e)
        {
            SetToolTips();
            PopulateSettingsList();

            List<ListItem> lstBuildMethods = new List<ListItem>(4)
            {
                new ListItem(CharacterBuildMethod.Priority, LanguageManager.GetString("String_Priority")),
                new ListItem(CharacterBuildMethod.SumtoTen, LanguageManager.GetString("String_SumtoTen")),
                new ListItem(CharacterBuildMethod.Karma, LanguageManager.GetString("String_Karma"))
            };
            if (GlobalSettings.LifeModuleEnabled)
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.LifeModule, LanguageManager.GetString("String_LifeModule")));

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.PopulateWithListItems(lstBuildMethods);
            cboBuildMethod.EndUpdate();

            PopulateOptions();
            SetupDataBindings();

            IsDirty = false;
            _blnLoading = false;
            _blnIsLayoutSuspended = false;
        }

        #endregion Form Events

        #region Control Events

        private void cmdGlobalOptionsCustomData_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            using (frmGlobalSettings frmOptions = new frmGlobalSettings("tabCustomDataDirectories"))
                frmOptions.ShowDialog(this);
        }

        private void cmdRename_Click(object sender, EventArgs e)
        {
            using (frmSelectText frmSelectName = new frmSelectText
            {
                DefaultString = _objCharacterSettings.Name,
                Description = LanguageManager.GetString("Message_CharacterOptions_SettingRename")
            })
            {
                frmSelectName.ShowDialog(this);
                if (frmSelectName.DialogResult != DialogResult.OK)
                    return;
                _objCharacterSettings.Name = frmSelectName.SelectedValue;
            }

            using (new CursorWait(this))
            {
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (cboSetting.SelectedIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem = new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value, _objCharacterSettings.DisplayName);
                    _blnLoading = true;
                    cboSetting.BeginUpdate();
                    _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
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
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to delete this setting
            if (Program.MainForm.ShowMessageBox(
                string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_CharacterOptions_ConfirmDelete"),
                    _objReferenceCharacterSettings.Name),
                LanguageManager.GetString("MessageTitle_Options_ConfirmDelete"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (!Utils.SafeDeleteFile(Path.Combine(Application.StartupPath, "settings", _objReferenceCharacterSettings.FileName), true))
                return;

            using (new CursorWait(this))
            {
                SettingsManager.LoadedCharacterSettingsAsModifiable.Remove(_objReferenceCharacterSettings.DictionaryKey);
                // Force repopulate character settings list in Master Index from here in lieu of event handling for concurrent dictionaries
                _blnForceMasterIndexRepopulateOnClose = true;
                KeyValuePair<string, CharacterSettings> kvpReplacementOption =
                    SettingsManager.LoadedCharacterSettings.First(x => x.Value.BuiltInOption
                                                                     && x.Value.BuildMethod ==
                                                                     _objReferenceCharacterSettings.BuildMethod);
                foreach (Character objCharacter in Program.MainForm.OpenCharacters.Where(x =>
                    x.SettingsKey == _objReferenceCharacterSettings.FileName))
                    objCharacter.SettingsKey = kvpReplacementOption.Key;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterSettings = kvpReplacementOption.Value;
                _objCharacterSettings.CopyValues(_objReferenceCharacterSettings);
                RebuildCustomDataDirectoryInfos();
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
            string strSelectedFullFileName;
            do
            {
                do
                {
                    using (frmSelectText frmSelectName = new frmSelectText
                    {
                        DefaultString = _objCharacterSettings.BuiltInOption
                            ? string.Empty
                            : _objCharacterSettings.FileName.TrimEndOnce(".xml"),
                        Description = LanguageManager.GetString("Message_CharacterOptions_SelectSettingName")
                    })
                    {
                        frmSelectName.ShowDialog(this);
                        if (frmSelectName.DialogResult != DialogResult.OK)
                            return;
                        strSelectedName = frmSelectName.SelectedValue;
                    }

                    if (SettingsManager.LoadedCharacterSettings.Any(x => x.Value.Name == strSelectedName))
                    {
                        DialogResult eCreateDuplicateSetting = Program.MainForm.ShowMessageBox(
                            string.Format(LanguageManager.GetString("Message_CharacterOptions_DuplicateSettingName"),
                                strSelectedName),
                            LanguageManager.GetString("MessageTitle_CharacterOptions_DuplicateFileName"),
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        switch (eCreateDuplicateSetting)
                        {
                            case DialogResult.Cancel:
                                return;

                            case DialogResult.No:
                                strSelectedName = string.Empty;
                                break;
                        }
                    }
                } while (string.IsNullOrWhiteSpace(strSelectedName));

                string strBaseFileName = strSelectedName.FastEscape(Path.GetInvalidFileNameChars()).TrimEndOnce(".xml");
                // Make sure our file name isn't too long, otherwise we run into problems on Windows
                // We can assume that Chummer's startup path plus 16 is within the limit, otherwise the user would have had problems installing Chummer with its data files in the first place
                int intStartupPathLimit = Application.StartupPath.Length + 16;
                if (strBaseFileName.Length > intStartupPathLimit)
                    strBaseFileName = strBaseFileName.Substring(0, intStartupPathLimit);
                strSelectedFullFileName = strBaseFileName + ".xml";
                int intMaxNameLength = char.MaxValue - Application.StartupPath.Length - "settings".Length - 6;
                uint uintAccumulator = 1;
                string strSeparator = "_";
                while (SettingsManager.LoadedCharacterSettings.Any(x => x.Value.FileName == strSelectedFullFileName))
                {
                    strSelectedFullFileName = strBaseFileName + strSeparator + uintAccumulator.ToString(GlobalSettings.InvariantCultureInfo) + ".xml";
                    if (strSelectedFullFileName.Length > intMaxNameLength)
                    {
                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_CharacterOptions_SettingFileNameTooLongError"),
                            LanguageManager.GetString("MessageTitle_CharacterOptions_SettingFileNameTooLongError"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        strSelectedName = string.Empty;
                        break;
                    }
                    if (uintAccumulator == uint.MaxValue)
                    {
                        uintAccumulator = 0;
                        strSeparator += '_';
                    }
                    uintAccumulator += 1;
                }
            } while (string.IsNullOrWhiteSpace(strSelectedName));

            using (new CursorWait(this))
            {
                _objCharacterSettings.Name = strSelectedName;
                if (!_objCharacterSettings.Save(strSelectedFullFileName, true))
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                CharacterSettings objNewCharacterSettings = new CharacterSettings();
                objNewCharacterSettings.CopyValues(_objCharacterSettings);
                SettingsManager.LoadedCharacterSettingsAsModifiable.Add(
                    objNewCharacterSettings.DictionaryKey,
                    objNewCharacterSettings);
                // Force repopulate character settings list in Master Index from here in lieu of event handling for concurrent dictionaries
                _blnForceMasterIndexRepopulateOnClose = true;
                _objReferenceCharacterSettings = objNewCharacterSettings;
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
                if (_objReferenceCharacterSettings.BuildMethod != _objCharacterSettings.BuildMethod)
                {
                    StringBuilder sbdConflictingCharacters = new StringBuilder();
                    foreach (Character objCharacter in Program.MainForm.OpenCharacters)
                    {
                        if (!objCharacter.Created && ReferenceEquals(objCharacter.Settings, _objReferenceCharacterSettings))
                            sbdConflictingCharacters.AppendLine(objCharacter.CharacterName);
                    }
                    if (sbdConflictingCharacters.Length > 0)
                    {
                        Program.MainForm.ShowMessageBox(this,
                            LanguageManager.GetString("Message_CharacterOptions_OpenCharacterOnBuildMethodChange") +
                            sbdConflictingCharacters,
                            LanguageManager.GetString("MessageTitle_CharacterOptions_OpenCharacterOnBuildMethodChange"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (!_objCharacterSettings.Save())
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterSettings.CopyValues(_objCharacterSettings);
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
            if (string.IsNullOrEmpty(strSelectedFile) || !SettingsManager.LoadedCharacterSettings.TryGetValue(strSelectedFile, out CharacterSettings objNewOption))
                return;

            if (IsDirty)
            {
                string text = LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty");
                string caption = LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty");

                if (Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                {
                    _blnLoading = true;
                    cboSetting.SelectedIndex = _intOldSelectedSettingIndex;
                    _blnLoading = false;
                    return;
                }
                IsDirty = false;
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

                if (_blnWasRenamed && _intOldSelectedSettingIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem =
                        new ListItem(_lstSettings[_intOldSelectedSettingIndex].Value, _objReferenceCharacterSettings.DisplayName);
                    cboSetting.BeginUpdate();
                    _lstSettings[_intOldSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
                    cboSetting.EndUpdate();
                }

                _objReferenceCharacterSettings = objNewOption;
                _objCharacterSettings.CopyValues(objNewOption);
                RebuildCustomDataDirectoryInfos();
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cmdRestoreDefaults_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if (Program.MainForm.ShowMessageBox(
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

                if (_blnWasRenamed && cboSetting.SelectedIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem =
                        new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value, _objReferenceCharacterSettings.DisplayName);
                    cboSetting.BeginUpdate();
                    _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
                    cboSetting.EndUpdate();
                }

                _objCharacterSettings.CopyValues(_objReferenceCharacterSettings);
                RebuildCustomDataDirectoryInfos();
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cboLimbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || _blnSkipLimbCountUpdate)
                return;

            string strLimbCount = cboLimbCount.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strLimbCount))
            {
                _objCharacterSettings.LimbCount = 6;
                _objCharacterSettings.ExcludeLimbSlot = string.Empty;
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if (intSeparatorIndex == -1)
                {
                    if (int.TryParse(strLimbCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intLimbCount))
                        _objCharacterSettings.LimbCount = intLimbCount;
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterSettings.LimbCount = 6;
                    }
                    _objCharacterSettings.ExcludeLimbSlot = string.Empty;
                }
                else
                {
                    if (int.TryParse(strLimbCount.Substring(0, intSeparatorIndex), NumberStyles.Any,
                        GlobalSettings.InvariantCultureInfo, out int intLimbCount))
                    {
                        _objCharacterSettings.LimbCount = intLimbCount;
                        _objCharacterSettings.ExcludeLimbSlot = intSeparatorIndex + 1 < strLimbCount.Length ? strLimbCount.Substring(intSeparatorIndex + 1) : string.Empty;
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterSettings.LimbCount = 6;
                        _objCharacterSettings.ExcludeLimbSlot = string.Empty;
                    }
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmCharacterSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDirty && Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty"),
                LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
            }

            if (_blnForceMasterIndexRepopulateOnClose)
            {
                Program.MainForm.MasterIndex?.ForceRepopulateCharacterSettings();
            }
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            _blnLoading = true;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                string strBookCode = objNode.Tag.ToString();
                if (!_setPermanentSourcebooks.Contains(strBookCode))
                {
                    objNode.Checked = _blnSourcebookToggle;
                    if (_blnSourcebookToggle)
                        _objCharacterSettings.BooksWritable.Add(strBookCode);
                    else
                        _objCharacterSettings.BooksWritable.Remove(strBookCode);
                }
            }
            _blnLoading = false;
            _objCharacterSettings.RecalculateBookXPath();
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.Books));
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
                _objCharacterSettings.BooksWritable.Add(strBookCode);
            else
                _objCharacterSettings.BooksWritable.Remove(strBookCode);
            _objCharacterSettings.RecalculateBookXPath();
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.Books));
        }

        private void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex - 1, 2);
            _objCharacterSettings.CustomDataDirectoryKeys.Reverse(intIndex - 1, 2);
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdToTopCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            for (int i = intIndex; i > 0; --i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i - 1, 2);
                _objCharacterSettings.CustomDataDirectoryKeys.Reverse(i - 1, 2);
            }
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex, 2);
            _objCharacterSettings.CustomDataDirectoryKeys.Reverse(intIndex, 2);
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdToBottomCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            for (int i = intIndex; i < _dicCharacterCustomDataDirectoryInfos.Count - 1; ++i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i, 2);
                _objCharacterSettings.CustomDataDirectoryKeys.Reverse(i, 2);
            }
            _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            int intIndex = objNode.Index;
            _dicCharacterCustomDataDirectoryInfos[_dicCharacterCustomDataDirectoryInfos[intIndex].Key] = objNode.Checked;
            switch (objNode.Tag)
            {
                case CustomDataDirectoryInfo objCustomDataDirectoryInfo when _objCharacterSettings.CustomDataDirectoryKeys.ContainsKey(objCustomDataDirectoryInfo.CharacterSettingsSaveKey):
                    _objCharacterSettings.CustomDataDirectoryKeys[objCustomDataDirectoryInfo.CharacterSettingsSaveKey] = objNode.Checked;
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
                    break;
                case string strCustomDataDirectoryKey when _objCharacterSettings.CustomDataDirectoryKeys.ContainsKey(strCustomDataDirectoryKey):
                    _objCharacterSettings.CustomDataDirectoryKeys[strCustomDataDirectoryKey] = objNode.Checked;
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.CustomDataDirectoryKeys));
                    break;
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
            txtPriorities.ForeColor = txtPriorities.Text.Length == 5 ? ColorManager.WindowText : ColorManager.ErrorColor;
        }

        private void txtContactPoints_TextChanged(object sender, EventArgs e)
        {
            txtContactPoints.ForeColor = CommonFunctions.IsCharacterAttributeXPathValidOrNull(txtContactPoints.Text) ?
                ColorManager.WindowText : ColorManager.ErrorColor;
        }

        private void txtKnowledgePoints_TextChanged(object sender, EventArgs e)
        {
            txtKnowledgePoints.ForeColor = CommonFunctions.IsCharacterAttributeXPathValidOrNull(txtKnowledgePoints.Text) ?
                ColorManager.WindowText : ColorManager.ErrorColor;
        }

        private void txtNuyenExpression_TextChanged(object sender, EventArgs e)
        {
            txtNuyenExpression.ForeColor = CommonFunctions.IsCharacterAttributeXPathValidOrNull(txtNuyenExpression.Text) ?
                ColorManager.WindowText : ColorManager.ErrorColor;
        }
        private void txtBoundSpiritLimit_TextChanged(object sender, EventArgs e)
        {
            txtBoundSpiritLimit.ForeColor = CommonFunctions.IsCharacterAttributeXPathValidOrNull(txtBoundSpiritLimit.Text) ?
                ColorManager.WindowText : ColorManager.ErrorColor;
        }
        private void txtRegisteredSpriteLimit_TextChanged(object sender, EventArgs e)
        {
            txtRegisteredSpriteLimit.ForeColor = CommonFunctions.IsCharacterAttributeXPathValidOrNull(txtRegisteredSpriteLimit.Text) ?
                ColorManager.WindowText : ColorManager.ErrorColor;
        }

        private void chkGrade_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox chkGrade))
                return;

            string strGrade = chkGrade.Tag.ToString();
            if (chkGrade.Checked)
            {
                if (_objCharacterSettings.BannedWareGrades.Contains(strGrade))
                {
                    _objCharacterSettings.BannedWareGrades.Remove(strGrade);
                    _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.BannedWareGrades));
                }
            }
            else if (!_objCharacterSettings.BannedWareGrades.Contains(strGrade))
            {
                _objCharacterSettings.BannedWareGrades.Add(strGrade);
                _objCharacterSettings.OnPropertyChanged(nameof(CharacterSettings.BannedWareGrades));
            }
        }

        private void cboPriorityTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strNewPriorityTable = cboPriorityTable.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(strNewPriorityTable))
                return;
            _objCharacterSettings.PriorityTable = strNewPriorityTable;
        }

        private void treCustomDataDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node?.Tag is CustomDataDirectoryInfo objSelected))
            {
                gpbDirectoryInfo.Visible = false;
                return;
            }

            gpbDirectoryInfo.SuspendLayout();
            txtDirectoryDescription.Text = objSelected.DisplayDescription;
            lblDirectoryVersion.Text = objSelected.MyVersion.ToString();
            lblDirectoryAuthors.Text = objSelected.DisplayAuthors;
            lblDirectoryName.Text = objSelected.Name;

            if (objSelected.DependenciesList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var dependency in objSelected.DependenciesList)
                    sb.AppendLine(dependency.DisplayName);
                lblDependencies.Text = sb.ToString();
            }
            else
            {
                //Make sure all old information is discarded
                lblDependencies.Text = string.Empty;
            }

            if (objSelected.IncompatibilitiesList.Count > 0)
            {
                //We only need a Stringbuilder if we got anything
                StringBuilder sb = new StringBuilder();
                foreach (var exclusivity in objSelected.IncompatibilitiesList)
                    sb.AppendLine(exclusivity.DisplayName);
                lblIncompatibilities.Text = sb.ToString();
            }
            else
            {
                lblIncompatibilities.Text = string.Empty;
            }
            gpbDirectoryInfo.Visible = true;
            gpbDirectoryInfo.ResumeLayout();
        }

        #endregion Control Events

        #region Methods

        private void PopulateSourcebookTreeView()
        {
            // Load the Sourcebook information.
            // Put the Sourcebooks into a List so they can first be sorted.
            object objOldSelected = treSourcebook.SelectedNode?.Tag;
            treSourcebook.BeginUpdate();
            treSourcebook.Nodes.Clear();
            _setPermanentSourcebooks.Clear();
            foreach (XPathNavigator objXmlBook in XmlManager.LoadXPath("books.xml", _objCharacterSettings.EnabledCustomDataDirectoryPaths).SelectAndCacheExpression("/chummer/books/book"))
            {
                if (objXmlBook.SelectSingleNodeAndCacheExpression("hide") != null)
                    continue;
                string strCode = objXmlBook.SelectSingleNodeAndCacheExpression("code")?.Value;
                if (string.IsNullOrEmpty(strCode))
                    continue;
                bool blnChecked = _objCharacterSettings.Books.Contains(strCode);
                if (objXmlBook.SelectSingleNodeAndCacheExpression("permanent") != null)
                {
                    _setPermanentSourcebooks.Add(strCode);
                    _objCharacterSettings.BooksWritable.Add(strCode);
                    blnChecked = true;
                }
                TreeNode objNode = new TreeNode
                {
                    Text = objXmlBook.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlBook.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty,
                    Tag = strCode,
                    Checked = blnChecked
                };
                treSourcebook.Nodes.Add(objNode);
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
            if (_dicCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (KeyValuePair<object, bool> kvpInfo in _dicCharacterCustomDataDirectoryInfos)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Tag = kvpInfo.Key,
                        Checked = kvpInfo.Value
                    };
                    if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.DisplayName;
                        if (objNode.Checked)
                        {
                            // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                            string missingDirectories = string.Empty;
                            if (objInfo.DependenciesList.Count > 0)
                                missingDirectories = objInfo.CheckDependency(_objCharacterSettings);

                            string prohibitedDirectories = string.Empty;
                            if (objInfo.IncompatibilitiesList.Count > 0)
                                prohibitedDirectories = objInfo.CheckIncompatibility(_objCharacterSettings);

                            if (!string.IsNullOrEmpty(missingDirectories) || !string.IsNullOrEmpty(prohibitedDirectories))
                            {
                                objNode.ToolTipText = CustomDataDirectoryInfo.BuildIncompatibilityDependencyString(missingDirectories, prohibitedDirectories);
                                objNode.ForeColor = ColorManager.ErrorColor;
                            }
                        }
                    }
                    else
                    {
                        objNode.Text = kvpInfo.Key.ToString();
                        objNode.ForeColor = ColorManager.GrayText;
                        objNode.ToolTipText = LanguageManager.GetString("MessageTitle_FileNotFound");
                    }
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for (int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataDirectories.Nodes[i];
                    KeyValuePair<object, bool> kvpInfo = _dicCharacterCustomDataDirectoryInfos[i];
                    if (!kvpInfo.Key.Equals(objNode.Tag))
                        objNode.Tag = kvpInfo.Key;
                    if (kvpInfo.Value != objNode.Checked)
                        objNode.Checked = kvpInfo.Value;
                    if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.DisplayName;
                        if (objNode.Checked)
                        {
                            // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                            string missingDirectories = string.Empty;
                            if (objInfo.DependenciesList.Count > 0)
                                missingDirectories = objInfo.CheckDependency(_objCharacterSettings);

                            string prohibitedDirectories = string.Empty;
                            if (objInfo.IncompatibilitiesList.Count > 0)
                                prohibitedDirectories = objInfo.CheckIncompatibility(_objCharacterSettings);

                            if (!string.IsNullOrEmpty(missingDirectories) || !string.IsNullOrEmpty(prohibitedDirectories))
                            {
                                objNode.ToolTipText = CustomDataDirectoryInfo.BuildIncompatibilityDependencyString(missingDirectories, prohibitedDirectories);
                                objNode.ForeColor = ColorManager.ErrorColor;
                            }
                            else
                            {
                                objNode.ToolTipText = string.Empty;
                                objNode.ForeColor = ColorManager.WindowText;
                            }
                        }
                        else
                        {
                            objNode.ToolTipText = string.Empty;
                            objNode.ForeColor = ColorManager.WindowText;
                        }
                    }
                    else
                    {
                        objNode.Text = kvpInfo.Key.ToString();
                        objNode.ForeColor = ColorManager.GrayText;
                        objNode.ToolTipText = LanguageManager.GetString("MessageTitle_FileNotFound");
                    }
                }
            }

            if (objOldSelected != null)
                treCustomDataDirectories.SelectedNode = treCustomDataDirectories.FindNodeByTag(objOldSelected);
            treCustomDataDirectories.ShowNodeToolTips = true;
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
            PopulateCustomDataDirectoryTreeView();
            if (blnDoResumeLayout)
            {
                _blnIsLayoutSuspended = false;
                ResumeLayout();
            }
        }

        private void PopulatePriorityTableList()
        {
            List<ListItem> lstPriorityTables = new List<ListItem>();

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("priorities.xml", _objCharacterSettings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/prioritytables/prioritytable"))
            {
                string strName = objXmlNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    lstPriorityTables.Add(new ListItem(objXmlNode.Value, objXmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
            }

            string strOldSelected = _objCharacterSettings.PriorityTable;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            cboPriorityTable.BeginUpdate();
            cboPriorityTable.PopulateWithListItems(lstPriorityTables);
            if (!string.IsNullOrEmpty(strOldSelected))
                cboPriorityTable.SelectedValue = strOldSelected;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedValue = _objReferenceCharacterSettings.PriorityTable;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedIndex = 0;
            cboPriorityTable.EndUpdate();
            _blnLoading = blnOldLoading;
            string strSelectedTable = cboPriorityTable.SelectedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(strSelectedTable) && _objCharacterSettings.PriorityTable != strSelectedTable)
                _objCharacterSettings.PriorityTable = strSelectedTable;
        }

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("options.xml", _objCharacterSettings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/limbcounts/limb"))
            {
                string strExclude = objXmlNode.SelectSingleNodeAndCacheExpression("exclude")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strExclude))
                    strExclude = '<' + strExclude;
                lstLimbCount.Add(new ListItem(objXmlNode.SelectSingleNodeAndCacheExpression("limbcount")?.Value + strExclude, objXmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty));
            }

            string strLimbSlot = _objCharacterSettings.LimbCount.ToString(GlobalSettings.InvariantCultureInfo);
            if (!string.IsNullOrEmpty(_objCharacterSettings.ExcludeLimbSlot))
                strLimbSlot += '<' + _objCharacterSettings.ExcludeLimbSlot;

            _blnSkipLimbCountUpdate = true;
            cboLimbCount.BeginUpdate();
            cboLimbCount.PopulateWithListItems(lstLimbCount);
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

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("bioware.xml", _objCharacterSettings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/grades/grade[not(hide)]"))
            {
                string strName = objXmlNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                if (!string.IsNullOrEmpty(strName) && strName != "None")
                {
                    string strBook = objXmlNode.SelectSingleNodeAndCacheExpression("source")?.Value;
                    if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                        continue;
                    if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                        continue;
                    ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                    if (objExistingCoveredGrade.Value != null)
                        lstGrades.Remove(objExistingCoveredGrade);
                    lstGrades.Add(new ListItem(strName, objXmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                }
            }
            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("cyberware.xml", _objCharacterSettings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/grades/grade[not(hide)]"))
            {
                string strName = objXmlNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                if (!string.IsNullOrEmpty(strName) && strName != "None")
                {
                    string strBook = objXmlNode.SelectSingleNodeAndCacheExpression("source")?.Value;
                    if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                        continue;
                    if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                        continue;
                    ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                    if (objExistingCoveredGrade.Value != null)
                        lstGrades.Remove(objExistingCoveredGrade);
                    lstGrades.Add(new ListItem(strName, objXmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
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
                    Checked = !_objCharacterSettings.BannedWareGrades.Contains(objGrade.Value.ToString())
                };
                chkGrade.CheckedChanged += chkGrade_CheckedChanged;
                flpAllowedCyberwareGrades.Controls.Add(chkGrade);
            }
            flpAllowedCyberwareGrades.ResumeLayout();
        }

        private void RebuildCustomDataDirectoryInfos()
        {
            _dicCharacterCustomDataDirectoryInfos.Clear();
            foreach (KeyValuePair<string, bool> kvpCustomDataDirectory in _objCharacterSettings.CustomDataDirectoryKeys)
            {
                CustomDataDirectoryInfo objLoopInfo = GlobalSettings.CustomDataDirectoryInfos.FirstOrDefault(x => x.CharacterSettingsSaveKey == kvpCustomDataDirectory.Key);
                if (objLoopInfo != default)
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(objLoopInfo, kvpCustomDataDirectory.Value);
                }
                else
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(kvpCustomDataDirectory.Key, kvpCustomDataDirectory.Value);
                }
            }
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
            cmdRename.DoOneWayNegatableDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.BuiltInOption));
            cmdDelete.DoOneWayNegatableDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.BuiltInOption));

            cboBuildMethod.DoDataBinding("SelectedValue", _objCharacterSettings, nameof(CharacterSettings.BuildMethod));
            lblPriorityTable.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodUsesPriorityTables));
            cboPriorityTable.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodUsesPriorityTables));
            lblPriorities.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodIsPriority));
            txtPriorities.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodIsPriority));
            txtPriorities.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.PriorityArray));
            lblSumToTen.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodIsSumtoTen));
            nudSumToTen.DoOneWayDataBinding("Visible", _objCharacterSettings, nameof(CharacterSettings.BuildMethodIsSumtoTen));
            nudSumToTen.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.SumtoTen));
            nudStartingKarma.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.BuildKarma));
            nudMaxNuyenKarma.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenMaximumBP));
            nudMaxAvail.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.MaximumAvailability));
            nudQualityKarmaLimit.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.QualityKarmaLimit));
            txtContactPoints.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.ContactPointsExpression));
            txtKnowledgePoints.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.KnowledgePointsExpression));
            txtNuyenExpression.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.ChargenKarmaToNuyenExpression));
            txtRegisteredSpriteLimit.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.RegisteredSpriteExpression));
            txtBoundSpiritLimit.DoDataBinding("Text", _objCharacterSettings, nameof(CharacterSettings.BoundSpiritExpression));

            chkEnforceCapacity.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.EnforceCapacity));
            chkLicenseEachRestrictedItem.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.LicenseRestricted));
            chkReverseAttributePriorityOrder.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ReverseAttributePriorityOrder));
            chkDronemods.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DroneMods));
            chkDronemodsMaximumPilot.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DroneModsMaximumPilot));
            chkRestrictRecoil.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.RestrictRecoil));
            chkStrictSkillGroups.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.StrictSkillGroupsInCreateMode));
            chkAllowPointBuySpecializationsOnKarmaSkills.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowPointBuySpecializationsOnKarmaSkills));
            chkAllowFreeGrids.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowFreeGrids));

            chkDontUseCyberlimbCalculation.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DontUseCyberlimbCalculation));
            chkCyberlegMovement.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.CyberlegMovement));
            chkCyberlimbAttributeBonusCap.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.CyberlimbAttributeBonusCap));
            chkRedlinerLimbsSkull.DoNegatableDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.RedlinerExcludesSkull));
            chkRedlinerLimbsTorso.DoNegatableDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.RedlinerExcludesTorso));
            chkRedlinerLimbsArms.DoNegatableDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.RedlinerExcludesArms));
            chkRedlinerLimbsLegs.DoNegatableDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.RedlinerExcludesLegs));

            nudNuyenDecimalsMaximum.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.MaxNuyenDecimals));
            nudNuyenDecimalsMinimum.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.MinNuyenDecimals));
            nudEssenceDecimals.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.EssenceDecimals));
            chkDontRoundEssenceInternally.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DontRoundEssenceInternally));

            chkEnable4eStyleEnemyTracking.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.EnableEnemyTracking));
            flpKarmaGainedFromEnemies.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.EnableEnemyTracking));
            nudKarmaGainedFromEnemies.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaEnemy));
            chkEnemyKarmaQualityLimit.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.EnableEnemyTracking));
            chkEnemyKarmaQualityLimit.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.EnemyKarmaQualityLimit));
            chkMoreLethalGameplay.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.MoreLethalGameplay));

            chkNoArmorEncumbrance.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.NoArmorEncumbrance));
            chkIgnoreArt.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.IgnoreArt));
            chkIgnoreComplexFormLimit.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.IgnoreComplexFormLimit));
            chkUnarmedSkillImprovements.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.UnarmedImprovementsApplyToWeapons));
            chkMysAdPp.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.MysAdeptAllowPpCareer));
            chkMysAdPp.DoOneWayNegatableDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.MysAdeptSecondMAGAttribute));
            chkPrioritySpellsAsAdeptPowers.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.PrioritySpellsAsAdeptPowers));
            chkPrioritySpellsAsAdeptPowers.DoOneWayNegatableDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.MysAdeptSecondMAGAttributeEnabled));
            chkUsePointsOnBrokenGroups.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.UsePointsOnBrokenGroups));
            chkSpecialKarmaCost.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.SpecialKarmaCostBasedOnShownValue));
            chkUseCalculatedPublicAwareness.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.UseCalculatedPublicAwareness));
            chkAlternateMetatypeAttributeKarma.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AlternateMetatypeAttributeKarma));
            chkCompensateSkillGroupKarmaDifference.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.CompensateSkillGroupKarmaDifference));
            chkFreeMartialArtSpecialization.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.FreeMartialArtSpecialization));
            chkIncreasedImprovedAbilityModifier.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.IncreasedImprovedAbilityMultiplier));
            chkAllowTechnomancerSchooling.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowTechnomancerSchooling));
            chkAllowSkillRegrouping.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowSkillRegrouping));
            chkDontDoubleQualityPurchases.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DontDoubleQualityPurchases));
            chkDontDoubleQualityRefunds.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DontDoubleQualityRefunds));
            chkDroneArmorMultiplier.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.DroneArmorMultiplier));
            chkESSLossReducesMaximumOnly.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ESSLossReducesMaximumOnly));
            chkExceedNegativeQualities.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ExceedNegativeQualitiesLimit));
            chkExceedPositiveQualities.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoOneWayDataBinding("Enabled", _objCharacterSettings, nameof(CharacterSettings.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ExceedPositiveQualitiesCostDoubled));
            chkExtendAnyDetectionSpell.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.ExtendAnyDetectionSpell));
            chkAllowCyberwareESSDiscounts.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowCyberwareESSDiscounts));
            chkAllowInitiation.DoDataBinding("Checked", _objCharacterSettings, nameof(CharacterSettings.AllowInitiationInCreateMode));

            // Karma options.
            nudMetatypeCostsKarmaMultiplier.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.MetatypeCostsKarmaMultiplier));
            nudKarmaNuyenPerWftM.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenPerBPWftM));
            nudKarmaNuyenPerWftP.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.NuyenPerBPWftP));
            nudKarmaAttribute.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaAttribute));
            nudKarmaQuality.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaQuality));
            nudKarmaSpecialization.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpecialization));
            nudKarmaKnowledgeSpecialization.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaKnowledgeSpecialization));
            nudKarmaNewKnowledgeSkill.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewKnowledgeSkill));
            nudKarmaNewActiveSkill.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewActiveSkill));
            nudKarmaNewSkillGroup.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewSkillGroup));
            nudKarmaImproveKnowledgeSkill.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaImproveKnowledgeSkill));
            nudKarmaImproveActiveSkill.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaImproveActiveSkill));
            nudKarmaImproveSkillGroup.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaImproveSkillGroup));
            nudKarmaSpell.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpell));
            nudKarmaNewComplexForm.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewComplexForm));
            nudKarmaNewAIProgram.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewAIProgram));
            nudKarmaNewAIAdvancedProgram.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaNewAIAdvancedProgram));
            nudKarmaMetamagic.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaMetamagic));
            nudKarmaContact.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaContact));
            nudKarmaCarryover.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaCarryover));
            nudKarmaSpirit.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpirit));
            nudKarmaSpiritFettering.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpiritFettering));
            nudKarmaTechnique.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaTechnique));
            nudKarmaInitiation.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaInitiation));
            nudKarmaInitiationFlat.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaInitiationFlat));
            nudKarmaJoinGroup.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaJoinGroup));
            nudKarmaLeaveGroup.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaLeaveGroup));
            nudKarmaMysticAdeptPowerPoint.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaMysticAdeptPowerPoint));

            // Focus costs
            nudKarmaAlchemicalFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaAlchemicalFocus));
            nudKarmaBanishingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaBanishingFocus));
            nudKarmaBindingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaBindingFocus));
            nudKarmaCenteringFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaCenteringFocus));
            nudKarmaCounterspellingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaCounterspellingFocus));
            nudKarmaDisenchantingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaDisenchantingFocus));
            nudKarmaFlexibleSignatureFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaFlexibleSignatureFocus));
            nudKarmaMaskingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaMaskingFocus));
            nudKarmaPowerFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaPowerFocus));
            nudKarmaQiFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaQiFocus));
            nudKarmaRitualSpellcastingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaRitualSpellcastingFocus));
            nudKarmaSpellcastingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpellcastingFocus));
            nudKarmaSpellShapingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSpellShapingFocus));
            nudKarmaSummoningFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSummoningFocus));
            nudKarmaSustainingFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaSustainingFocus));
            nudKarmaWeaponFocus.DoDataBinding("Value", _objCharacterSettings, nameof(CharacterSettings.KarmaWeaponFocus));

            _objCharacterSettings.PropertyChanged += SettingsChanged;
        }

        private void PopulateSettingsList()
        {
            string strSelect = string.Empty;
            if (!_blnLoading)
                strSelect = cboSetting.SelectedValue?.ToString();
            cboSetting.BeginUpdate();
            _lstSettings.Clear();
            foreach (KeyValuePair<string, CharacterSettings> kvpCharacterSettingsEntry in SettingsManager.LoadedCharacterSettings)
            {
                _lstSettings.Add(new ListItem(kvpCharacterSettingsEntry.Key, kvpCharacterSettingsEntry.Value.DisplayName));
                if (ReferenceEquals(_objReferenceCharacterSettings, kvpCharacterSettingsEntry.Value))
                    strSelect = kvpCharacterSettingsEntry.Key;
            }
            _lstSettings.Sort(CompareListItems.CompareNames);
            cboSetting.PopulateWithListItems(_lstSettings);
            if (!string.IsNullOrEmpty(strSelect))
                cboSetting.SelectedValue = strSelect;
            if (cboSetting.SelectedIndex == -1 && _lstSettings.Count > 0)
                cboSetting.SelectedValue = cboSetting.FindStringExact(GlobalSettings.DefaultCharacterSetting);
            if (cboSetting.SelectedIndex == -1 && _lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;
            cboSetting.EndUpdate();
            _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_blnLoading)
            {
                IsDirty = !_objCharacterSettings.Equals(_objReferenceCharacterSettings);
                cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
                cmdSave.Enabled = cmdSaveAs.Enabled && !_objCharacterSettings.BuiltInOption;
                switch (e.PropertyName)
                {
                    case nameof(CharacterSettings.EnabledCustomDataDirectoryPaths):
                        PopulateOptions();
                        break;

                    case nameof(CharacterSettings.PriorityTable):
                        PopulatePriorityTableList();
                        break;
                }
            }
            else
            {
                switch (e.PropertyName)
                {
                    case nameof(CharacterSettings.BuiltInOption):
                        cmdSave.Enabled = cmdSaveAs.Enabled
                                          && !_objCharacterSettings.BuiltInOption;
                        break;

                    case nameof(CharacterSettings.PriorityArray):
                    case nameof(CharacterSettings.BuildMethod):
                        cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
                        cmdSave.Enabled = cmdSaveAs.Enabled
                                          && !_objCharacterSettings.BuiltInOption;
                        break;
                }
            }
        }

        private bool IsAllTextBoxesLegal
        {
            get
            {
                if (_objCharacterSettings.BuildMethod == CharacterBuildMethod.Priority && _objCharacterSettings.PriorityArray.Length != 5)
                    return false;

                return CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                           _objCharacterSettings.ContactPointsExpression) &&
                       CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                           _objCharacterSettings.KnowledgePointsExpression) &&
                       CommonFunctions.IsCharacterAttributeXPathValidOrNull(
                           _objCharacterSettings.ChargenKarmaToNuyenExpression.Replace("{Karma}", "0")
                               .Replace("{PriorityNuyen}", "0"));
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
                    {
                        _blnWasRenamed = false;
                        cmdSaveAs.Enabled = false;
                        cmdSave.Enabled = false;
                    }
                }
            }
        }

        #endregion Methods
    }
}
