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
    public partial class SelectArt : Form
    {
        private string _strSelectedItem = string.Empty;

        private bool _blnLoading = true;
        private readonly string _strBaseXPath = "art";
        private readonly string _strXPathFilter = string.Empty;
        private readonly string _strLocalName = string.Empty;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _objXmlDocument;

        public enum Mode
        {
            Art = 0,
            Enhancement,
            Enchantment,
            Ritual
        }

        public SelectArt(Character objCharacter, Mode objWindowMode)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Load the Metamagic information.
            switch (objWindowMode)
            {
                case Mode.Art:
                    _objXmlDocument = objCharacter.LoadDataXPath("metamagic.xml").SelectSingleNodeAndCacheExpression("/chummer/arts");
                    _strLocalName = LanguageManager.GetString("String_Art");
                    _strBaseXPath = "art";
                    _strXPathFilter = _objCharacter.Settings.BookXPath();
                    break;

                case Mode.Enhancement:
                    _objXmlDocument = objCharacter.LoadDataXPath("powers.xml").SelectSingleNodeAndCacheExpression("/chummer/enhancements");
                    _strLocalName = LanguageManager.GetString("String_Enhancement");
                    _strBaseXPath = "enhancement";
                    _strXPathFilter = _objCharacter.Settings.BookXPath();
                    break;

                case Mode.Enchantment:
                    _strLocalName = LanguageManager.GetString("String_Enchantment");
                    _objXmlDocument = objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Enchantments' and (" + _objCharacter.Settings.BookXPath() + ')';
                    break;

                case Mode.Ritual:
                    _strLocalName = LanguageManager.GetString("String_Ritual");
                    _objXmlDocument = objCharacter.LoadDataXPath("spells.xml").SelectSingleNodeAndCacheExpression("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Rituals' and (" + _objCharacter.Settings.BookXPath() + ')';
                    break;
            }
        }

        private async void SelectArt_Load(object sender, EventArgs e)
        {
            string strText = string.Format(GlobalSettings.CultureInfo,
                                           await LanguageManager.GetStringAsync("Title_SelectGeneric").ConfigureAwait(false), _strLocalName);
            await this.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
            string strLimitText = string.Format(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(
                                                    "Checkbox_SelectGeneric_LimitList").ConfigureAwait(false), _strLocalName);
            await chkLimitList.DoThreadSafeAsync(x => x.Text = strLimitText).ConfigureAwait(false);

            _blnLoading = false;

            await BuildList().ConfigureAwait(false);
        }

        private async void lstArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelected = await lstArt.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelected))
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            // Retrieve the information for the selected art
            XPathNavigator objXmlMetamagic = _objXmlDocument.TryGetNodeByNameOrId(_strBaseXPath, strSelected);

            if (objXmlMetamagic == null)
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            string strSource = objXmlMetamagic.SelectSingleNodeAndCacheExpression("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
            string strPage = objXmlMetamagic.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetamagic.SelectSingleNodeAndCacheExpression("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
            await objSource.SetControlAsync(lblSource, this).ConfigureAwait(false);
            await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void lstArt_DoubleClick(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        private async void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            await BuildList().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildList().ConfigureAwait(false);
        }

        #region Properties

        /// <summary>
        /// Id of the Art that was selected in the dialogue.
        /// </summary>
        public string SelectedItem => _strSelectedItem;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Build the list of Arts.
        /// </summary>
        private async Task BuildList(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strFilter = _strXPathFilter;
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstArts))
            {
                bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlMetamagic in
                         _objXmlDocument.Select(_strBaseXPath + '[' + strFilter + ']'))
                {
                    string strId = objXmlMetamagic.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strId)
                        && (!blnLimitList || await objXmlMetamagic.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false)))
                    {
                        lstArts.Add(new ListItem(objXmlMetamagic.SelectSingleNodeAndCacheExpression("id", token: token)?.Value,
                                                 objXmlMetamagic.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                 ?? objXmlMetamagic.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                    }
                }

                lstArts.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstArt.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstArt.PopulateWithListItemsAsync(lstArts, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstArt.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedItem = await lstArt.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XPathNavigator objXmlMetamagic = _objXmlDocument.TryGetNodeByNameOrId(_strBaseXPath, strSelectedItem);

                if (objXmlMetamagic == null || !await objXmlMetamagic.RequirementsMetAsync(_objCharacter, strLocalName: _strLocalName, token: token).ConfigureAwait(false))
                    return;

                _strSelectedItem = strSelectedItem;
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, token).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
