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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class frmSelectBuildMethod : Form
    {
        private readonly Character _objCharacter;
        private readonly CharacterBuildMethod _eStartingBuildMethod;
        private bool _blnLoading = true;
        private readonly bool _blnForExistingCharacter;

        #region Control Events

        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _eStartingBuildMethod = _objCharacter.Settings.BuildMethod;
            _blnForExistingCharacter = blnUseCurrentValues;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            // Populate the Character Settings list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCharacterSettings))
            {
                foreach (CharacterSettings objLoopSetting in SettingsManager.LoadedCharacterSettings.Values)
                {
                    lstCharacterSettings.Add(new ListItem(objLoopSetting, objLoopSetting.DisplayName));
                }

                lstCharacterSettings.Sort(CompareListItems.CompareNames);
                cboCharacterSetting.BeginUpdate();
                cboCharacterSetting.PopulateWithListItems(lstCharacterSettings);
                if (blnUseCurrentValues)
                {
                    if (SettingsManager.LoadedCharacterSettings.TryGetValue(
                            _objCharacter.SettingsKey, out CharacterSettings objSetting))
                        cboCharacterSetting.SelectedValue = objSetting;
                    if (cboCharacterSetting.SelectedIndex == -1
                        && SettingsManager.LoadedCharacterSettings.TryGetValue(
                            GlobalSettings.DefaultCharacterSetting, out objSetting))
                        cboCharacterSetting.SelectedValue = objSetting;
                    chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                }
                else if (SettingsManager.LoadedCharacterSettings.TryGetValue(
                             GlobalSettings.DefaultCharacterSetting, out CharacterSettings objSetting))
                    cboCharacterSetting.SelectedValue = objSetting;

                if (cboCharacterSetting.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                    cboCharacterSetting.SelectedIndex = 0;
                cboCharacterSetting.EndUpdate();
            }

            chkIgnoreRules.SetToolTip(LanguageManager.GetString("Tip_SelectKarma_IgnoreRules"));
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!(cboCharacterSetting.SelectedValue is CharacterSettings objSelectedGameplayOption))
                return;
            CharacterBuildMethod eSelectedBuildMethod = objSelectedGameplayOption.BuildMethod;
            if (_blnForExistingCharacter && !_objCharacter.Created && _objCharacter.Settings.BuildMethod == _objCharacter.EffectiveBuildMethod && eSelectedBuildMethod != _eStartingBuildMethod)
            {
                if (Program.MainForm.ShowMessageBox(this,
                    string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_SelectBP_SwitchBuildMethods"),
                        LanguageManager.GetString("String_" + eSelectedBuildMethod), LanguageManager.GetString("String_" + _eStartingBuildMethod)).WordWrap(),
                    LanguageManager.GetString("MessageTitle_SelectBP_SwitchBuildMethods"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                string strOldCharacterSettingsKey = _objCharacter.SettingsKey;
                _objCharacter.SettingsKey = SettingsManager.LoadedCharacterSettings
                    .First(x => ReferenceEquals(x.Value, objSelectedGameplayOption)).Key;
                // If the character is loading, make sure we only switch build methods after we've loaded, otherwise we might cause all sorts of nastiness
                if (_objCharacter.IsLoading)
                    _objCharacter.PostLoadMethods.Enqueue(() => _objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey));
                else if (!_objCharacter.SwitchBuildMethods(_eStartingBuildMethod, eSelectedBuildMethod, strOldCharacterSettingsKey))
                    return;
            }
            else
            {
                _objCharacter.SettingsKey = SettingsManager.LoadedCharacterSettings
                    .First(x => ReferenceEquals(x.Value, objSelectedGameplayOption)).Key;
            }
            _objCharacter.IgnoreRules = chkIgnoreRules.Checked;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdEditCharacterOption_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                using (EditCharacterSettings frmOptions =
                    new EditCharacterSettings(cboCharacterSetting.SelectedValue as CharacterSettings))
                    frmOptions.ShowDialogSafe(this);

                SuspendLayout();
                // Populate the Gameplay Settings list.
                object objOldSelected = cboCharacterSetting.SelectedValue;
                using (new FetchSafelyFromPool<List<ListItem>>(
                           Utils.ListItemListPool, out List<ListItem> lstGameplayOptions))
                {
                    lstGameplayOptions.AddRange(SettingsManager.LoadedCharacterSettings.Values
                                                               .Select(objLoopOptions =>
                                                                           new ListItem(
                                                                               objLoopOptions,
                                                                               objLoopOptions.DisplayName)));
                    lstGameplayOptions.Sort(CompareListItems.CompareNames);
                    cboCharacterSetting.BeginUpdate();
                    cboCharacterSetting.PopulateWithListItems(lstGameplayOptions);
                    cboCharacterSetting.SelectedValue = objOldSelected;
                    if (cboCharacterSetting.SelectedIndex == -1 && lstGameplayOptions.Count > 0
                                                                && SettingsManager.LoadedCharacterSettings.TryGetValue(
                                                                    GlobalSettings.DefaultCharacterSetting,
                                                                    out CharacterSettings objSetting))
                        cboCharacterSetting.SelectedValue = objSetting;
                    if (cboCharacterSetting.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                        cboCharacterSetting.SelectedIndex = 0;
                    cboCharacterSetting.EndUpdate();
                }

                ResumeLayout();
            }
        }

        private void frmSelectBuildMethod_Load(object sender, EventArgs e)
        {
            cboGamePlay_SelectedIndexChanged(this, e);
            _blnLoading = false;
        }

        private void cboGamePlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                SuspendLayout();
            // Load the Priority information.
            if (cboCharacterSetting.SelectedValue is CharacterSettings objSelectedGameplayOption)
            {
                lblBuildMethod.Text = LanguageManager.GetString("String_" + objSelectedGameplayOption.BuildMethod);
                switch (objSelectedGameplayOption.BuildMethod)
                {
                    case CharacterBuildMethod.Priority:
                        lblBuildMethodParamLabel.Text = LanguageManager.GetString("Label_SelectBP_Priorities");
                        lblBuildMethodParam.Text = objSelectedGameplayOption.PriorityArray;
                        lblBuildMethodParamLabel.Visible = true;
                        lblBuildMethodParam.Visible = true;
                        break;

                    case CharacterBuildMethod.SumtoTen:
                        lblBuildMethodParamLabel.Text = LanguageManager.GetString("String_SumtoTen");
                        lblBuildMethodParam.Text = objSelectedGameplayOption.SumtoTen.ToString(GlobalSettings.CultureInfo);
                        lblBuildMethodParamLabel.Visible = true;
                        lblBuildMethodParam.Visible = true;
                        break;

                    default:
                        lblBuildMethodParamLabel.Visible = false;
                        lblBuildMethodParam.Visible = false;
                        break;
                }

                lblMaxAvail.Text = objSelectedGameplayOption.MaximumAvailability.ToString(GlobalSettings.CultureInfo);
                lblKarma.Text = objSelectedGameplayOption.BuildKarma.ToString(GlobalSettings.CultureInfo);
                lblMaxNuyen.Text = objSelectedGameplayOption.NuyenMaximumBP.ToString(GlobalSettings.CultureInfo);
                lblQualityKarma.Text = objSelectedGameplayOption.QualityKarmaLimit.ToString(GlobalSettings.CultureInfo);

                lblBooks.Text = _objCharacter.TranslatedBookList(string.Join(";", objSelectedGameplayOption.Books));
                if (string.IsNullOrEmpty(lblBooks.Text))
                    lblBooks.Text = LanguageManager.GetString("String_None");

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdCustomDataDirectories))
                {
                    foreach (CustomDataDirectoryInfo objLoopInfo in objSelectedGameplayOption
                                 .EnabledCustomDataDirectoryInfos)
                        sbdCustomDataDirectories.AppendLine(objLoopInfo.Name);

                    lblCustomData.Text = sbdCustomDataDirectories.ToString();
                }

                if (string.IsNullOrEmpty(lblBooks.Text))
                    lblCustomData.Text = LanguageManager.GetString("String_None");
            }

            if (!_blnLoading)
                ResumeLayout();
        }

        #endregion Control Events
    }
}
