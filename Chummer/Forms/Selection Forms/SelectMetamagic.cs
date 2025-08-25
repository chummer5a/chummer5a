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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectMetamagic : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedMetamagic = string.Empty;
        private readonly bool _blnTechnomancer;
        private readonly int _intMyGrade;
        private string _strType;
        private readonly string _strRootXPath;

        private readonly Character _objCharacter;

        private XPathNavigator _objXmlDocument;

        private readonly List<string> _lstMetamagicLimits = new List<string>();

        #region Control Events

        public SelectMetamagic(Character objCharacter, InitiationGrade objGrade)
        {
            if (objGrade == null)
                throw new ArgumentNullException(nameof(objGrade));
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _blnTechnomancer = objGrade.Technomancer;
            _intMyGrade = objGrade.Grade;
            _strRootXPath = _blnTechnomancer
                ? "/chummer/echoes/echo"
                : "/chummer/metamagics/metamagic";
        }

        private async void SelectMetamagic_Load(object sender, EventArgs e)
        {
            // Load the Metamagic information.
            if (_blnTechnomancer)
            {
                _objXmlDocument = await _objCharacter.LoadDataXPathAsync("echoes.xml").ConfigureAwait(false);
                _strType = await LanguageManager.GetStringAsync("String_Echo").ConfigureAwait(false);
            }
            else
            {
                _objXmlDocument = await _objCharacter.LoadDataXPathAsync("metamagic.xml").ConfigureAwait(false);
                _strType = await LanguageManager.GetStringAsync("String_Metamagic").ConfigureAwait(false);
            }
            foreach (Improvement imp in await ImprovementManager
                                        .GetCachedImprovementListForValueOfAsync(
                                            _objCharacter, Improvement.ImprovementType.MetamagicLimit).ConfigureAwait(false))
            {
                if (imp.Rating == _intMyGrade)
                    _lstMetamagicLimits.Add(imp.ImprovedName);
            }
            string strText = string.Format(GlobalSettings.CultureInfo,
                                           await LanguageManager.GetStringAsync("Title_SelectGeneric").ConfigureAwait(false), _strType);
            await this.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
            string strLimitText = string.Format(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync(
                                                    "Checkbox_SelectGeneric_LimitList").ConfigureAwait(false), _strType);
            await chkLimitList.DoThreadSafeAsync(x => x.Text = strLimitText).ConfigureAwait(false);

            _blnLoading = false;
            await BuildMetamagicList().ConfigureAwait(false);
        }

        private async void lstMetamagic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstMetamagic.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected metamagic/echo.
                XPathNavigator objXmlMetamagic = _objXmlDocument.TryGetNodeByNameOrId(_strRootXPath, strSelectedId);

                if (objXmlMetamagic != null)
                {
                    string strSource = objXmlMetamagic.SelectSingleNodeAndCacheExpression("source")?.Value;
                    string strPage = objXmlMetamagic.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlMetamagic.SelectSingleNodeAndCacheExpression("page")?.Value;
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSourceString.ToString())).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void lstMetamagic_DoubleClick(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
        }

        private async void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            await BuildMetamagicList().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildMetamagicList().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Id of Metamagic that was selected in the dialogue.
        /// </summary>
        public string SelectedMetamagic => _strSelectedMetamagic;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Build the list of Metamagics.
        /// </summary>
        private async Task BuildMetamagicList(CancellationToken token = default)
        {
            string strFilter = '(' + await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false) + ')';
            // If the character has MAG enabled, filter the list based on Adept/Magician availability.
            if (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
            {
                bool blnIsMagician = await _objCharacter.GetMagicianEnabledAsync(token).ConfigureAwait(false);
                if (blnIsMagician != await _objCharacter.GetAdeptEnabledAsync(token).ConfigureAwait(false))
                {
                    if (blnIsMagician)
                        strFilter += "and magician = " + bool.TrueString.CleanXPath();
                    else
                        strFilter += "and adept = " + bool.TrueString.CleanXPath();
                }
            }

            if (_lstMetamagicLimits.Count > 0)
            {
                strFilter += " and (";
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    foreach (string strMetamagic in _lstMetamagicLimits)
                        sbdFilter.Append("name = ").Append(strMetamagic.CleanXPath()).Append(" or ");
                    sbdFilter.Length -= 4;
                    strFilter += sbdFilter.ToString() + ')';
                }
            }

            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetamagics))
            {
                foreach (XPathNavigator objXmlMetamagic in
                         _objXmlDocument.Select(_strRootXPath + '[' + strFilter + ']'))
                {
                    string strId = objXmlMetamagic.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (!chkLimitList.Checked || await objXmlMetamagic.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                    {
                        lstMetamagics.Add(new ListItem(strId,
                                                       objXmlMetamagic.SelectSingleNodeAndCacheExpression("translate", token: token)
                                                                      ?.Value ?? objXmlMetamagic
                                                           .SelectSingleNodeAndCacheExpression("name", token: token)?.Value ??
                                                       await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                    }
                }

                lstMetamagics.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstMetamagic.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstMetamagic.PopulateWithListItemsAsync(lstMetamagics, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstMetamagic.DoThreadSafeAsync(x =>
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
            string strSelectedId = await lstMetamagic.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Make sure the selected Metamagic or Echo meets its requirements.
                XPathNavigator objXmlMetamagic = _objXmlDocument.TryGetNodeByNameOrId(_strRootXPath, strSelectedId);

                if (objXmlMetamagic == null || !await objXmlMetamagic.RequirementsMetAsync(_objCharacter, strLocalName: _strType, token: token).ConfigureAwait(false))
                    return;

                _strSelectedMetamagic = strSelectedId;
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
