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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectMartialArtTechnique : Form
    {
        private string _strSelectedTechnique = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;

        private readonly MartialArt _objMartialArt;
        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly Character _objCharacter;
        private readonly HashSet<string> _setAllowedTechniques = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectMartialArtTechnique(Character objCharacter, MartialArt objMartialArt)
        {
            Disposed += (sender, args) => Utils.StringHashSetPool.Return(_setAllowedTechniques);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objMartialArt = objMartialArt ?? throw new ArgumentNullException(nameof(objMartialArt));
            // Load the Martial Art information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNodeAndCacheExpression("/chummer");
            // Populate the Martial Art Technique list.
            XPathNavigator xmlMartialArtNode = _xmlBaseChummerNode?.SelectSingleNode("martialarts/martialart[name = " + _objMartialArt.Name.CleanXPath() + ']');
            if (xmlMartialArtNode != null)
            {
                if (!xmlMartialArtNode.NodeExists("alltechniques"))
                {
                    foreach (XPathNavigator xmlTechnique in xmlMartialArtNode.SelectAndCacheExpression("techniques/technique"))
                    {
                        string strTechniqueName = xmlTechnique.Value;
                        if (_objMartialArt.Techniques.All(x => x.Name != strTechniqueName))
                        {
                            _setAllowedTechniques.Add(strTechniqueName);
                        }
                    }
                }
                else if (_objMartialArt.Techniques.Count == 0)
                {
                    //TODO: Support for allowing all techniques > 0.
                    foreach (XPathNavigator xmlTechnique in _xmlBaseChummerNode.Select("techniques/technique[(" + _objCharacter.Settings.BookXPath() + ")]"))
                    {
                        string strTechnique = xmlTechnique.Value;
                        if (_objMartialArt.Techniques.Any(x => x.Name == strTechnique))
                            continue;
                        string strTechniqueName = xmlTechnique.SelectSingleNodeAndCacheExpression("name")?.Value;
                        if (string.IsNullOrEmpty(strTechniqueName))
                            continue;
                        _setAllowedTechniques.Add(strTechniqueName);
                    }
                }
            }
        }

        private async void SelectMartialArtTechnique_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            await RefreshTechniquesList().ConfigureAwait(false);
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

        private async void lstTechniques_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstTechniques.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XPathNavigator xmlTechnique = _xmlBaseChummerNode.SelectSingleNode("/chummer/techniques/technique[id = " + strSelectedId.CleanXPath() + ']');

                if (xmlTechnique != null)
                {
                    string strSource = (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("source").ConfigureAwait(false))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage = (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("altpage").ConfigureAwait(false))?.Value ?? (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("page").ConfigureAwait(false))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource).ConfigureAwait(false);
                    string strSourceText = lblSource.ToString();
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSourceText)).ConfigureAwait(false);
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

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshTechniquesList().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Martial Art Technique that was selected in the dialogue.
        /// </summary>
        public string SelectedTechnique => _strSelectedTechnique;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstTechniques.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedTechnique = strSelectedId;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Populate the Martial Arts Techniques list.
        /// </summary>
        private async ValueTask RefreshTechniquesList(CancellationToken token = default)
        {
            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false) + ')';
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            XPathNodeIterator objTechniquesList = _xmlBaseChummerNode.Select("techniques/technique[" + strFilter + ']');

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTechniqueItems))
            {
                foreach (XPathNavigator xmlTechnique in objTechniquesList)
                {
                    string strId = (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))?.Value;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        string strTechniqueName = (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                                  ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);

                        if (_setAllowedTechniques?.Contains(strTechniqueName) == false)
                            continue;

                        if (xmlTechnique.RequirementsMet(_objCharacter, _objMartialArt))
                        {
                            lstTechniqueItems.Add(new ListItem(
                                                      strId,
                                                      (await xmlTechnique.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))
                                                                  ?.Value ?? strTechniqueName));
                        }
                    }
                }

                lstTechniqueItems.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstTechniques.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstTechniques.PopulateWithListItemsAsync(lstTechniqueItems, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstTechniques.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
