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
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));

            // Load the Metamagic information.
            switch (objWindowMode)
            {
                case Mode.Art:
                    _objXmlDocument = objCharacter.LoadDataXPath("metamagic.xml").SelectSingleNode("/chummer/arts");
                    _strLocalName = LanguageManager.GetString("String_Art");
                    _strBaseXPath = "art";
                    _strXPathFilter = _objCharacter.Settings.BookXPath();
                    break;

                case Mode.Enhancement:
                    _objXmlDocument = objCharacter.LoadDataXPath("powers.xml").SelectSingleNode("/chummer/enhancements");
                    _strLocalName = LanguageManager.GetString("String_Enhancement");
                    _strBaseXPath = "enhancement";
                    _strXPathFilter = _objCharacter.Settings.BookXPath();
                    break;

                case Mode.Enchantment:
                    _strLocalName = LanguageManager.GetString("String_Enchantment");
                    _objXmlDocument = objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Enchantments' and (" + _objCharacter.Settings.BookXPath() + ')';
                    break;

                case Mode.Ritual:
                    _strLocalName = LanguageManager.GetString("String_Ritual");
                    _objXmlDocument = objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/spells");
                    _strBaseXPath = "spell";
                    _strXPathFilter = "category = 'Rituals' and (" + _objCharacter.Settings.BookXPath() + ')';
                    break;
            }
        }

        private async void SelectArt_Load(object sender, EventArgs e)
        {
            Text = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Title_SelectGeneric"), _strLocalName);
            chkLimitList.Text = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Checkbox_SelectGeneric_LimitList"), _strLocalName);

            _blnLoading = false;

            await BuildList();
        }

        private async void lstArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelected = lstArt.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelected))
            {
                tlpRight.Visible = false;
                return;
            }

            // Retrieve the information for the selected art
            XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strBaseXPath + "[id = " + strSelected.CleanXPath() + ']');

            if (objXmlMetamagic == null)
            {
                tlpRight.Visible = false;
                return;
            }

            string strSource = objXmlMetamagic.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            string strPage = (await objXmlMetamagic.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? objXmlMetamagic.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
            SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                                                                             GlobalSettings.CultureInfo, _objCharacter);
            lblSource.Text = objSource.ToString();
            await lblSource.SetToolTipAsync(objSource.LanguageBookTooltip);
            tlpRight.Visible = true;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstArt_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private async void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            await BuildList();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildList();
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
        private async ValueTask BuildList()
        {
            if (_blnLoading)
                return;
            
            string strFilter = _strXPathFilter;
            if (!string.IsNullOrEmpty(txtSearch.Text))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text);
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstArts))
            {
                foreach (XPathNavigator objXmlMetamagic in
                         _objXmlDocument.Select(_strBaseXPath + '[' + strFilter + ']'))
                {
                    string strId = (await objXmlMetamagic.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value;
                    if (!string.IsNullOrEmpty(strId)
                        && (!chkLimitList.Checked || objXmlMetamagic.RequirementsMet(_objCharacter)))
                    {
                        lstArts.Add(new ListItem((await objXmlMetamagic.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value,
                                                 (await objXmlMetamagic.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                                 ?? (await objXmlMetamagic.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown")));
                    }
                }

                lstArts.Sort(CompareListItems.CompareNames);
                string strOldSelected = lstArt.SelectedValue?.ToString();
                _blnLoading = true;
                await lstArt.PopulateWithListItemsAsync(lstArts);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstArt.SelectedValue = strOldSelected;
                else
                    lstArt.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstArt.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XPathNavigator objXmlMetamagic = _objXmlDocument.SelectSingleNode(_strBaseXPath + "[id = " + strSelectedItem.CleanXPath() + ']');

                if (objXmlMetamagic != null)
                {
                    if (!objXmlMetamagic.RequirementsMet(_objCharacter, null, _strLocalName))
                        return;

                    _strSelectedItem = strSelectedItem;

                    DialogResult = DialogResult.OK;
                }
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
