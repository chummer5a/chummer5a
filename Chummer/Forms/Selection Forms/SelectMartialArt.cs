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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectMartialArt : Form
    {
        private bool _blnLoading = true;
        private string _strSelectedMartialArt = string.Empty;

        private bool _blnAddAgain;
        private string _strForcedValue = string.Empty;

        private readonly XPathNavigator _xmlBaseMartialArtsNode;
        private readonly XPathNavigator _xmlBaseMartialArtsTechniquesNode;
        private readonly Character _objCharacter;

        #region Control Events

        public SelectMartialArt(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;

            // Load the Martial Arts information.
            XPathNavigator xmlBaseMartialArtsDocumentNode = _objCharacter.LoadDataXPath("martialarts.xml");
            _xmlBaseMartialArtsNode = xmlBaseMartialArtsDocumentNode.SelectSingleNode("/chummer/martialarts");
            _xmlBaseMartialArtsTechniquesNode = xmlBaseMartialArtsDocumentNode.SelectSingleNode("/chummer/techniques");
        }

        private async void SelectMartialArt_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_strForcedValue))
            {
                _blnAddAgain = false;
                string strSelectedId = _xmlBaseMartialArtsNode.SelectSingleNode("martialart[name = " + _strForcedValue.CleanXPath() + "]/id")?.Value;
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    _strSelectedMartialArt = strSelectedId;
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.DialogResult = DialogResult.OK;
                        x.Close();
                    });
                }
            }

            _blnLoading = false;

            await RefreshArtList();
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

        private void lstMartialArts_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private async void lstMartialArts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstMartialArts.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Populate the Martial Arts list.
                XPathNavigator objXmlArt = _xmlBaseMartialArtsNode.SelectSingleNode("martialart[id = " + strSelectedId.CleanXPath() + ']');

                if (objXmlArt != null)
                {
                    string strKarmaCost = (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("cost"))?.Value
                                          ?? 7.ToString(GlobalSettings.CultureInfo);
                    await lblKarmaCost.DoThreadSafeAsync(x => x.Text = strKarmaCost);
                    await lblKarmaCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strKarmaCost));

                    string strTechniques;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTechniques))
                    {
                        foreach (XPathNavigator xmlMartialArtsTechnique in await objXmlArt.SelectAndCacheExpressionAsync(
                                     "techniques/technique"))
                        {
                            string strLoopTechniqueName
                                = (await xmlMartialArtsTechnique.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strLoopTechniqueName))
                            {
                                XPathNavigator xmlTechniqueNode
                                    = _xmlBaseMartialArtsTechniquesNode.SelectSingleNode(
                                        "technique[name = " + strLoopTechniqueName.CleanXPath() + " and ("
                                        + await _objCharacter.Settings.BookXPathAsync() + ")]");
                                if (xmlTechniqueNode != null)
                                {
                                    if (sbdTechniques.Length > 0)
                                        sbdTechniques.AppendLine(',');
                                    sbdTechniques.Append(
                                        !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                        StringComparison.OrdinalIgnoreCase)
                                            ? (await xmlTechniqueNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                              ?? strLoopTechniqueName
                                            : strLoopTechniqueName);
                                }
                            }
                        }

                        strTechniques = sbdTechniques.ToString();
                    }

                    await lblIncludedTechniques.DoThreadSafeAsync(x => x.Text = strTechniques);
                    await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTechniques));

                    string strSource = (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("source"))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                    string strPage = (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("page"))?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                    await objSourceString.SetControlAsync(lblSource);
                    string strSourceText = objSourceString.ToString();
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSourceText));
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                    await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = false);
                }
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = false);
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshArtList();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Martial Art that was selected in the dialogue.
        /// </summary>
        public string SelectedMartialArt => _strSelectedMartialArt;

        /// <summary>
        /// Only show Martial Arts that are provided by a quality
        /// </summary>
        public bool ShowQualities { get; set; }

        /// <summary>
        /// Force a Martial Art to be selected.
        /// </summary>
        public string ForcedValue
        {
            set => _strForcedValue = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMartialArts.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedMartialArt = strSelectedId;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        /// <summary>
        /// Populate the Martial Arts list.
        /// </summary>
        private async ValueTask RefreshArtList(CancellationToken token = default)
        {
            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token) + ')';
            if (ShowQualities)
                strFilter += " and isquality = " + bool.TrueString.CleanXPath();
            else
                strFilter += " and not(isquality = " + bool.TrueString.CleanXPath() + ')';
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);

            XPathNodeIterator objArtList = _xmlBaseMartialArtsNode.Select("martialart[" + strFilter + ']');
            
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMartialArt))
            {
                foreach (XPathNavigator objXmlArt in objArtList)
                {
                    string strId = (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("id", token: token))?.Value;
                    if (!string.IsNullOrEmpty(strId) && objXmlArt.RequirementsMet(_objCharacter))
                    {
                        lstMartialArt.Add(new ListItem(
                                              strId,
                                              (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("translate", token: token))?.Value
                                              ?? (await objXmlArt.SelectSingleNodeAndCacheExpressionAsync("name", token: token))?.Value
                                              ?? await LanguageManager.GetStringAsync("String_Unknown", token: token)));
                    }
                }

                lstMartialArt.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstMartialArts.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token);
                _blnLoading = true;
                await lstMartialArts.PopulateWithListItemsAsync(lstMartialArt, token: token);
                _blnLoading = false;
                await lstMartialArts.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token);
            }
        }

        #endregion Methods
    }
}
