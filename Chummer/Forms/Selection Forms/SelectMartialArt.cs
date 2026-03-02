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
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Load the Martial Arts information.
            XPathNavigator xmlBaseMartialArtsDocumentNode = _objCharacter.LoadDataXPath("martialarts.xml");
            _xmlBaseMartialArtsNode = xmlBaseMartialArtsDocumentNode.SelectSingleNodeAndCacheExpression("/chummer/martialarts");
            _xmlBaseMartialArtsTechniquesNode = xmlBaseMartialArtsDocumentNode.SelectSingleNodeAndCacheExpression("/chummer/techniques");
        }

        private async void SelectMartialArt_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_strForcedValue))
            {
                _blnAddAgain = false;
                XPathNavigator xmlForcedMartialArtNode
                    = _xmlBaseMartialArtsNode.TryGetNodeByNameOrId("martialart", _strForcedValue);
                if (xmlForcedMartialArtNode != null)
                {
                    string strSelectedId = xmlForcedMartialArtNode.SelectSingleNodeAndCacheExpression("id")?.Value;
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        _strSelectedMartialArt = strSelectedId;
                        await this.DoThreadSafeAsync(x =>
                        {
                            x.DialogResult = DialogResult.OK;
                            x.Close();
                        }).ConfigureAwait(false);
                    }
                }
            }

            _blnLoading = false;

            await RefreshArtList().ConfigureAwait(false);
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

            string strSelectedId = await lstMartialArts.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Populate the Martial Arts list.
                XPathNavigator objXmlArt = _xmlBaseMartialArtsNode.TryGetNodeByNameOrId("martialart", strSelectedId);

                if (objXmlArt != null)
                {
                    string strKarmaCost = objXmlArt.SelectSingleNodeAndCacheExpression("cost")?.Value
                                          ?? 7.ToString(GlobalSettings.CultureInfo);
                    await lblKarmaCost.DoThreadSafeAsync(x => x.Text = strKarmaCost).ConfigureAwait(false);
                    await lblKarmaCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strKarmaCost)).ConfigureAwait(false);

                    string strTechniques;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTechniques))
                    {
                        foreach (XPathNavigator xmlMartialArtsTechnique in objXmlArt.SelectAndCacheExpression(
                                     "techniques/technique"))
                        {
                            string strLoopTechniqueName
                                = xmlMartialArtsTechnique.SelectSingleNodeAndCacheExpression("name")?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strLoopTechniqueName))
                            {
                                XPathNavigator xmlTechniqueNode
                                    = _xmlBaseMartialArtsTechniquesNode.SelectSingleNode(
                                        "technique[name = " + strLoopTechniqueName.CleanXPath() + " and "
                                        + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync().ConfigureAwait(false) + "]");
                                if (xmlTechniqueNode != null)
                                {
                                    if (sbdTechniques.Length > 0)
                                        sbdTechniques.AppendLine(',');
                                    sbdTechniques.Append(
                                        !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                        StringComparison.OrdinalIgnoreCase)
                                            ? xmlTechniqueNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                              ?? strLoopTechniqueName
                                            : strLoopTechniqueName);
                                }
                            }
                        }

                        strTechniques = sbdTechniques.ToString();
                    }

                    await lblIncludedTechniques.DoThreadSafeAsync(x => x.Text = strTechniques).ConfigureAwait(false);
                    await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTechniques)).ConfigureAwait(false);

                    string strSource = objXmlArt.SelectSingleNodeAndCacheExpression("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage = objXmlArt.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlArt.SelectSingleNodeAndCacheExpression("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource, this).ConfigureAwait(false);
                    string strSourceText = await objSourceString.ToStringAsync().ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strSourceText)).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                }
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                await gpbIncludedTechniques.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshArtList().ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
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
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        /// <summary>
        /// Populate the Martial Arts list.
        /// </summary>
        private async Task RefreshArtList(CancellationToken token = default)
        {
            string strFilter = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);
            if (ShowQualities)
                strFilter += " and isquality = " + bool.TrueString.CleanXPath();
            else
                strFilter += " and not(isquality = " + bool.TrueString.CleanXPath() + ")";
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            if (!string.IsNullOrEmpty(strFilter))
                strFilter = "[" + strFilter + "]";
            XPathNodeIterator objArtList = _xmlBaseMartialArtsNode.Select("martialart" + strFilter);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMartialArt))
            {
                foreach (XPathNavigator objXmlArt in objArtList)
                {
                    string strId = objXmlArt.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strId) && await objXmlArt.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                    {
                        lstMartialArt.Add(new ListItem(
                                              strId,
                                              objXmlArt.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                              ?? objXmlArt.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                              ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                    }
                }

                lstMartialArt.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstMartialArts.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                _blnLoading = true;
                await lstMartialArts.PopulateWithListItemsAsync(lstMartialArt, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstMartialArts.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
