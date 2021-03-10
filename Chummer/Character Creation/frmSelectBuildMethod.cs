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
        private readonly bool _blnLockBuildMethod;
        private readonly CharacterBuildMethod _eStartingBuildMethod;
        private bool _blnLoading = true;

        #region Control Events
        public frmSelectBuildMethod(Character objCharacter, bool blnUseCurrentValues = false)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _eStartingBuildMethod = _objCharacter.Options.BuildMethod;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            // Populate the Gameplay Options list.
            List<ListItem> lstGameplayOptions = new List<ListItem>(OptionsManager.LoadedCharacterOptions.Count);
            foreach (KeyValuePair<string, CharacterOptions> objLoopOptions in OptionsManager.LoadedCharacterOptions)
            {
                lstGameplayOptions.Add(new ListItem(objLoopOptions.Value, objLoopOptions.Value.DisplayName));
            }
            lstGameplayOptions.Sort(CompareListItems.CompareNames);
            cboCharacterOption.BeginUpdate();
            cboCharacterOption.ValueMember = nameof(ListItem.Value);
            cboCharacterOption.DisplayMember = nameof(ListItem.Name);
            cboCharacterOption.DataSource = lstGameplayOptions;
            if (blnUseCurrentValues)
            {
                cboCharacterOption.SelectedValue = OptionsManager.LoadedCharacterOptions[_objCharacter.CharacterOptionsKey];
                if (cboCharacterOption.SelectedIndex == -1)
                    cboCharacterOption.SelectedValue = OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];
                chkIgnoreRules.Checked = _objCharacter.IgnoreRules;
                _blnLockBuildMethod = !_objCharacter.Created && _objCharacter.Options.BuildMethod == _objCharacter.EffectiveBuildMethod;
            }
            else
                cboCharacterOption.SelectedValue = OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];
            if (cboCharacterOption.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                cboCharacterOption.SelectedIndex = 0;
            cboCharacterOption.EndUpdate();

            chkIgnoreRules.SetToolTip(LanguageManager.GetString("Tip_SelectKarma_IgnoreRules"));
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!(cboCharacterOption.SelectedValue is CharacterOptions objSelectedGameplayOption))
                return;

            _objCharacter.CharacterOptionsKey = OptionsManager.LoadedCharacterOptions.First(x => x.Value == objSelectedGameplayOption).Key;
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
            Cursor = Cursors.WaitCursor;

            using (frmCharacterOptions frmOptions = new frmCharacterOptions(cboCharacterOption.SelectedValue as CharacterOptions))
                frmOptions.ShowDialog(this);

            SuspendLayout();
            // Populate the Gameplay Options list.
            object objOldSelected = cboCharacterOption.SelectedValue;
            List<ListItem> lstGameplayOptions = new List<ListItem>();
            foreach (KeyValuePair<string, CharacterOptions> objLoopOptions in OptionsManager.LoadedCharacterOptions)
            {
                lstGameplayOptions.Add(new ListItem(objLoopOptions.Value, objLoopOptions.Value.DisplayName));
            }
            lstGameplayOptions.Sort(CompareListItems.CompareNames);
            cboCharacterOption.BeginUpdate();
            cboCharacterOption.DataSource = lstGameplayOptions;
            cboCharacterOption.SelectedValue = objOldSelected;
            if (cboCharacterOption.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                cboCharacterOption.SelectedValue = OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];
            if (cboCharacterOption.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                cboCharacterOption.SelectedIndex = 0;
            cboCharacterOption.EndUpdate();
            ResumeLayout();

            Cursor = Cursors.Default;
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
            CharacterOptions objSelectedGameplayOption = cboCharacterOption.SelectedValue as CharacterOptions;
            if (objSelectedGameplayOption != null)
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
                        lblBuildMethodParam.Text = objSelectedGameplayOption.SumtoTen.ToString(GlobalOptions.CultureInfo);
                        lblBuildMethodParamLabel.Visible = true;
                        lblBuildMethodParam.Visible = true;
                        break;
                    default:
                        lblBuildMethodParamLabel.Visible = false;
                        lblBuildMethodParam.Visible = false;
                        break;
                }

                lblMaxAvail.Text = objSelectedGameplayOption.MaximumAvailability.ToString(GlobalOptions.CultureInfo);
                lblKarma.Text = objSelectedGameplayOption.BuildKarma.ToString(GlobalOptions.CultureInfo);
                lblMaxNuyen.Text = objSelectedGameplayOption.NuyenMaximumBP.ToString(GlobalOptions.CultureInfo);
                lblQualityKarma.Text = objSelectedGameplayOption.QualityKarmaLimit.ToString(GlobalOptions.CultureInfo);

                lblBooks.Text = _objCharacter.TranslatedBookList(string.Join(";", objSelectedGameplayOption.Books));
                if (string.IsNullOrEmpty(lblBooks.Text))
                    lblBooks.Text = LanguageManager.GetString("String_None");

                StringBuilder sbdCustomDataDirectories = new StringBuilder();
                foreach (CustomDataDirectoryInfo objLoopInfo in objSelectedGameplayOption.EnabledCustomDataDirectoryInfos)
                    sbdCustomDataDirectories.AppendLine(objLoopInfo.Name);

                lblCustomData.Text = sbdCustomDataDirectories.ToString();
                if (string.IsNullOrEmpty(lblBooks.Text))
                    lblCustomData.Text = LanguageManager.GetString("String_None");
            }

            if (_blnLockBuildMethod)
                cmdOK.Enabled = objSelectedGameplayOption?.BuildMethod == _eStartingBuildMethod;
            if (!_blnLoading)
                ResumeLayout();
        }
        #endregion
    }
}
