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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectComplexForm : Form
    {
        private string _strSelectedComplexForm = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBaseComplexFormsNode;
        private readonly XPathNavigator _xmlOptionalComplexFormNode;

        //private bool _blnBiowireEnabled = false;

        #region Control Events

        public SelectComplexForm(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Complex Form information.
            _xmlBaseComplexFormsNode = _objCharacter.LoadDataXPath("complexforms.xml").SelectSingleNode("/chummer/complexforms");

            _xmlOptionalComplexFormNode = _objCharacter.GetNodeXPath();
            if (_xmlOptionalComplexFormNode == null) return;
            if (_objCharacter.MetavariantGuid != Guid.Empty)
            {
                XPathNavigator xmlMetavariantNode = _xmlOptionalComplexFormNode.SelectSingleNode("metavariants/metavariant[id = "
                                                                                                 + _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                                                                                 + ']');
                if (xmlMetavariantNode != null)
                    _xmlOptionalComplexFormNode = xmlMetavariantNode;
            }

            _xmlOptionalComplexFormNode = _xmlOptionalComplexFormNode.SelectSingleNode("optionalcomplexforms");
        }

        private async void SelectComplexForm_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            await BuildComplexFormList();
        }

        private async void lstComplexForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedComplexFormId = await lstComplexForms.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (_blnLoading || string.IsNullOrEmpty(strSelectedComplexFormId))
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }

            // Display the Complex Form information.
            XPathNavigator xmlComplexForm = _xmlBaseComplexFormsNode.SelectSingleNode("complexform[id = " + strSelectedComplexFormId.CleanXPath() + ']');
            if (xmlComplexForm == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                string strDuration;
                switch (xmlComplexForm.SelectSingleNode("duration")?.Value)
                {
                    case "P":
                        strDuration = await LanguageManager.GetStringAsync("String_SpellDurationPermanent");
                        break;

                    case "S":
                        strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSustained");
                        break;

                    case "Special":
                        strDuration = await LanguageManager.GetStringAsync("String_SpellDurationSpecial");
                        break;

                    default:
                        strDuration = await LanguageManager.GetStringAsync("String_SpellDurationInstant");
                        break;
                }

                await lblDuration.DoThreadSafeAsync(x => x.Text = strDuration);

                string strTarget;
                switch (xmlComplexForm.SelectSingleNode("target")?.Value)
                {
                    case "Persona":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetPersona");
                        break;

                    case "Device":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetDevice");
                        break;

                    case "File":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetFile");
                        break;

                    case "Self":
                        strTarget = await LanguageManager.GetStringAsync("String_SpellRangeSelf");
                        break;

                    case "Sprite":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetSprite");
                        break;

                    case "Host":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetHost");
                        break;

                    case "IC":
                        strTarget = await LanguageManager.GetStringAsync("String_ComplexFormTargetIC");
                        break;

                    default:
                        strTarget = await LanguageManager.GetStringAsync("String_None");
                        break;
                }

                await lblTarget.DoThreadSafeAsync(x => x.Text = strTarget);

                string strFv = xmlComplexForm.SelectSingleNode("fv")?.Value.Replace('/', '÷').Replace('*', '×')
                               ?? string.Empty;
                if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strFv = await strFv
                                  .CheapReplaceAsync(
                                      "L", () => LanguageManager.GetStringAsync("String_ComplexFormLevel"))
                                  .CheapReplaceAsync("Overflow damage",
                                                     () => LanguageManager.GetStringAsync("String_SpellOverflowDamage"))
                                  .CheapReplaceAsync("Damage Value",
                                                     () => LanguageManager.GetStringAsync("String_SpellDamageValue"))
                                  .CheapReplaceAsync(
                                      "Toxin DV", () => LanguageManager.GetStringAsync("String_SpellToxinDV"))
                                  .CheapReplaceAsync("Disease DV",
                                                     () => LanguageManager.GetStringAsync("String_SpellDiseaseDV"))
                                  .CheapReplaceAsync("Radiation Power",
                                                     () => LanguageManager.GetStringAsync(
                                                         "String_SpellRadiationPower"));
                }

                await lblFV.DoThreadSafeAsync(x => x.Text = strFv);

                string strSource = xmlComplexForm.SelectSingleNode("source")?.Value ??
                                   await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = (await xmlComplexForm.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ??
                                 xmlComplexForm.SelectSingleNode("page")?.Value ??
                                 await LanguageManager.GetStringAsync("String_Unknown");
                SourceString objSource = await SourceString.GetSourceStringAsync(
                    strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                string strSourceText = objSource.ToString();
                await lblSource.DoThreadSafeAsync(x => x.Text = strSourceText);
                await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip);
                await lblTargetLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTarget));
                await lblDurationLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDuration));
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSourceText));
                await lblFVLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strFv));
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout());
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildComplexFormList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstComplexForms.SelectedIndex == -1 && lstComplexForms.Items.Count > 0)
            {
                lstComplexForms.SelectedIndex = 0;
            }
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        int intNewIndex = lstComplexForms.SelectedIndex + 1;
                        if (intNewIndex >= lstComplexForms.Items.Count)
                            intNewIndex = 0;
                        if (lstComplexForms.Items.Count > 0)
                            lstComplexForms.SelectedIndex = intNewIndex;
                        break;
                    }
                case Keys.Up:
                    {
                        int intNewIndex = lstComplexForms.SelectedIndex - 1;
                        if (intNewIndex <= 0)
                            intNewIndex = lstComplexForms.Items.Count - 1;
                        if (lstComplexForms.Items.Count > 0)
                            lstComplexForms.SelectedIndex = intNewIndex;
                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Complex Form that was selected in the dialogue.
        /// </summary>
        public string SelectedComplexForm => _strSelectedComplexForm;

        #endregion Properties

        #region Methods

        private async ValueTask BuildComplexFormList(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token) + ')';
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            // Populate the Complex Form list.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstComplexFormItems))
            {
                foreach (XPathNavigator xmlComplexForm in _xmlBaseComplexFormsNode.Select(
                             "complexform[" + strFilter + ']'))
                {
                    string strId = (await xmlComplexForm.SelectSingleNodeAndCacheExpressionAsync("id", token: token))?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;

                    if (!xmlComplexForm.RequirementsMet(_objCharacter))
                        continue;

                    string strName = (await xmlComplexForm.SelectSingleNodeAndCacheExpressionAsync("name", token: token))?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown", token: token);
                    // If this is a Sprite with Optional Complex Forms, see if this Complex Form is allowed.
                    if (_xmlOptionalComplexFormNode != null
                        && await _xmlOptionalComplexFormNode.SelectSingleNodeAndCacheExpressionAsync("complexform", token: token) != null
                        && _xmlOptionalComplexFormNode.SelectSingleNode("complexform[. = " + strName.CleanXPath() + ']') == null)
                        continue;

                    lstComplexFormItems.Add(
                        new ListItem(strId, (await xmlComplexForm.SelectSingleNodeAndCacheExpressionAsync("translate", token: token))?.Value ?? strName));
                }

                lstComplexFormItems.Sort(CompareListItems.CompareNames);
                _blnLoading = true;
                string strOldSelected = await lstComplexForms.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                await lstComplexForms.PopulateWithListItemsAsync(lstComplexFormItems, token: token);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    await lstComplexForms.DoThreadSafeAsync(x => x.SelectedValue = strOldSelected, token: token);
                else
                    await lstComplexForms.DoThreadSafeAsync(x => x.SelectedIndex = -1, token: token);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstComplexForms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                _strSelectedComplexForm = strSelectedItem;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
